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
                // if we ever need to supply UNTRUSTEDDRAGDROP,
                // seek for its contents there: https://learn.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/platform-apis/ms537178(v=vs.85)
                // e.g. new RawDataDescriptor(ClipboardFormatID.UNTRUSTEDDRAGDROP, new byte[]{ 0x0D, 0x18, 0, 0 }) 
            ];
            // we cannot add FILECONTENTS descriptors at this stage as _fileListSource will only be called upon actual drop,
            // so it appears we'll have to do this in GetData
        }
        public bool IsAsynchronous { get; set; } = true;

        public event Action? AsyncBegin;
        public event Action? AsyncEnd;

    }
    
}
