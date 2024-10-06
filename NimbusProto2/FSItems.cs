﻿using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NimbusProto2
{
    public abstract class FSItem(string id, FSDirectory? parent) : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private DateTime _creationTime;
        private DateTime _lastModifiedTime;
       
        public event PropertyChangedEventHandler? PropertyChanged;

        public string ID { get; } = id;
        public FSDirectory? Parent { get; } = parent;
        public string Name {  get => _name; set { _name = value; EmitPropertyChanged(); } }
        public DateTime CreationTime { get => _creationTime; set { _creationTime = value; EmitPropertyChanged(); } }
        public DateTime LastModifiedTime { get => _lastModifiedTime; set { _lastModifiedTime = value; EmitPropertyChanged(); } }

        public abstract string DisplayType { get; }

        public abstract string ImageKey { get; }

        public string FullPath { get { return Utils.URIPathCombine(Parent?.FullPath ?? "/", Name); } }

        public virtual void UpdateWith(YADISK.ResourcesItem resource)
        {
            if(resource.name != null && Name != resource.name)
                Name = resource.name;
            if(CreationTime != resource.created)
                CreationTime = resource.created;
            if(LastModifiedTime != resource.modified)
                LastModifiedTime = resource.modified;
        }

        public static FSItem? CreateFrom(YADISK.ResourcesItem resource, FSDirectory? parentDirectory)
        {
            if (null == resource.resource_id)
                return null;

            FSItem? newItem = resource.type switch
            {
                "dir" => new FSDirectory(resource.resource_id!, parentDirectory),
                "file" => new FSFile(resource.resource_id!, parentDirectory),
                _ => null
            };

            newItem?.UpdateWith(resource);
            return newItem;
        }

        protected void EmitPropertyChanged([CallerMemberName] string propName = "unknown")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    };

    public class FSFile(string id, FSDirectory? parent) : FSItem(id, parent)
    {
        private string? _mimeType;
        private string? _publicURL;
        public string? MIMEType { get => _mimeType; set { _mimeType = value; EmitPropertyChanged(); EmitPropertyChanged("DisplayType"); } }
        public string? PublicURL { get => _publicURL; set { _publicURL = value; EmitPropertyChanged(); } }
        public override string DisplayType => _mimeType ?? "";

        public override string ImageKey => Constants.StockImageKeys.File;

        public override void UpdateWith(YADISK.ResourcesItem resource)
        {
            base.UpdateWith(resource);
            if (MIMEType != resource.mime_type)
                MIMEType = resource.mime_type;
            if(PublicURL != resource.public_url)
                PublicURL = resource.public_url;
        }
    }

    public class FSDirectory(string id, FSDirectory? parent) : FSItem(id, parent)
    {
        public override string DisplayType => "Папка с файлами";
        public BetterBindingList<FSItem> Children { get; } = [];

        public override string ImageKey => Constants.StockImageKeys.Folder;

        public void UpdateChildren(YADISK.ResourceList? resources)
        {
            if(null == resources) return;

            HashSet<FSItem> itemsGone = []; // set of items that are no longer found in the incoming resources list
            HashSet<string> existingIDs = []; // set of item ids that exist in the current FSDirectory children list

            foreach(var child in Children)
            {
                var matchingResource = (from r in resources.items where r.resource_id == child.ID select r).FirstOrDefault();

                if(null == matchingResource)
                    itemsGone.Add(child);
                else
                {
                    existingIDs.Add(child.ID);
                    child.UpdateWith(matchingResource);
                }
            }

            // quite inefficient but that's the price we pay for using BindingList;
            // BindingList takes care of its items' PropertyChanged events so that it's probably worth using
            foreach(var goneChild in itemsGone)
                Children.Remove(goneChild);

            // add new items
            foreach (var resource in from r in resources.items
                                 where !existingIDs.Contains(r.resource_id!)
                                 select r)
            {
                if(FSItem.CreateFrom(resource, this) is FSItem newItem)
                    Children.Add(newItem);
            }
        }

        public IEnumerable<FSDirectory> DirectoryChain
        {
            get
            {
                return (Parent?.DirectoryChain ?? []).Append(this);
            }
        }
    }


}