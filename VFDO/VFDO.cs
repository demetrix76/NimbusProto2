using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using FileListSource = System.Func<System.Collections.Generic.IEnumerable<VirtualFiles.FileSource>>; // should be thread safe

namespace VirtualFiles
{
    public class FileSource
    {
        public string Name { get; set; } = string.Empty;
        public Int64? Size { get; set; }
        public UInt32? Attributes { get; set; } // might be useful for representing a Directory? To be checked
        public DateTime? LastModified { get; set; }
        public Func<IStream?>? StreamSource { get; set; } // should be thread safe

        public bool IsDirectory
        {
            get { return 0 != (Attributes.GetValueOrDefault(0) & NatConstants.FILE_ATTRIBUTE_DIRECTORY); }
            set
            {
                if (value)
                    Attributes = Attributes.GetValueOrDefault(0) | NatConstants.FILE_ATTRIBUTE_DIRECTORY;
                else
                    Attributes = Attributes.GetValueOrDefault() & ~NatConstants.FILE_ATTRIBUTE_DIRECTORY;
            }
        }
    }

    public partial class VFDO
    {
        private FileListSource _fileListSource;
        private List<DataDescriptor> _dataDescriptors;
        public VFDO(FileListSource fileListSource)
        {
            _fileListSource = fileListSource;
            _dataDescriptors = [
                new FileGroupDescriptor(_fileListSource),
            ];
            // we cannot add FILECONTENTS descriptors at this stage as _fileListSource will only be called upon actual drop,
            // so it appears we'll have to do this in GetData
        }
        public bool IsAsynchronous { get; set; } = true;

        public event Action? AsyncBegin;
        public event Action? AsyncEnd;

    }
    
}
