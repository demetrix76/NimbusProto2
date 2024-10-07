using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using VirtualFiles.NativeTypes;

namespace VirtualFiles
{
    using FileListSource = Func<IEnumerable<FileSource>>; // should be thread safe, and, ideally, idempotent
    using HRESULT = int;
    internal abstract class DataDescriptor
    {
        public abstract FORMATETC FormatDescriptor { get; }

        public abstract (nint, HRESULT) GetData();

        public bool Match(FORMATETC formatetc)
        {
            var fmt = FormatDescriptor;
            return fmt.cfFormat == formatetc.cfFormat
                && 0 != (fmt.tymed & formatetc.tymed)
                && fmt.dwAspect == formatetc.dwAspect
                && fmt.lindex == formatetc.lindex;
        }
    }

    internal class FileGroupDescriptor(FileListSource fileListSource) : DataDescriptor
    {
        private static FORMATETC _formatDescriptor = new()
        {
            cfFormat = ClipboardFormatID.FILEDESCRIPTORW,
            dwAspect = DVASPECT.DVASPECT_CONTENT,
            lindex = -1,
            ptd = nint.Zero,
            tymed = TYMED.TYMED_HGLOBAL // supposedly freed by the external caller?
        };

        private FileListSource _fileListSource = fileListSource;
        public override FORMATETC FormatDescriptor => _formatDescriptor;

        public override (nint, HRESULT) GetData()
        {
            var fileList = _fileListSource().ToArray();

            // serialize FILEGROUPDESCRIPTOR/FILEDESCRIPTOR structures into a byte list,
            // then create an HGLOBAL from the data
            List<byte> buffer = [];

            var groupDescriptor = new NativeTypes.FILEGROUPDESCRIPTOR { cItems = (uint)fileList.Length };
            buffer.AddRange(NatHelpers.MarshalStruct(groupDescriptor));

            foreach (var fd in fileList.Select(MakeFileDescriptor))
                buffer.AddRange(NatHelpers.MarshalStruct(fd));

            var ptr = NatHelpers.ToHGLOBAL([.. buffer]);

            return (ptr, ptr == 0 ? NatConstants.E_FAIL: NatConstants.S_OK);
        }

        private static NativeTypes.FILEDESCRIPTOR MakeFileDescriptor(FileSource fileSource)
        {
            NativeTypes.FILEDESCRIPTOR fileDescriptor = new() { cFileName = fileSource.Name, dwFlags = NatConstants.FD_UNICODE | NatConstants.FD_PROGRESSUI };

            if(fileSource.LastModified.HasValue)
            {
                fileDescriptor.dwFlags |= NatConstants.FD_WRITESTIME;
                fileDescriptor.ftLastAccessTime = NatHelpers.MakeFileTime(fileSource.LastModified.Value);
            }

            if(fileSource.Attributes.HasValue)
            {
                fileDescriptor.dwFlags |= NatConstants.FD_ATTRIBUTES;
                fileDescriptor.dwFileAttributes = fileSource.Attributes.Value;
            }

            if(fileSource.Size.HasValue)
            {
                fileDescriptor.dwFlags |= NatConstants.FD_FILESIZE;
                fileDescriptor.nFileSizeLow = (uint)(fileSource.Size & 0xffffffff);
                fileDescriptor.nFileSizeHigh = (uint)(fileSource.Size >> 32);
            }

            return fileDescriptor;
        }

    }

    internal class FileContentsDescriptor : DataDescriptor
    {
        private readonly FileListSource _fileListSource;
        private readonly int _fileIndex;

        private FORMATETC _formatDescriptor = new()
        {
            cfFormat = ClipboardFormatID.FILECONTENTS,
            dwAspect = DVASPECT.DVASPECT_CONTENT,
            lindex = -1,
            ptd = nint.Zero,
            tymed = TYMED.TYMED_ISTREAM
        };

        public override FORMATETC FormatDescriptor => _formatDescriptor;

        public override (nint, HRESULT) GetData()
        {
            var stream = _fileListSource().ToArray()[_fileIndex]?.StreamSource?.Invoke();
            if(stream != null)
                return (Marshal.GetComInterfaceForObject<IStream, IStream>(stream), NatConstants.S_OK);
            return (nint.Zero, NatConstants.E_FAIL);
        }

        public FileContentsDescriptor(FileListSource fileListSource, int fileIndex)
        {
            _fileListSource = fileListSource;
            _fileIndex = fileIndex;
            _formatDescriptor.lindex = fileIndex;
        }
    }
    internal class RawDataDescriptor : DataDescriptor
    {
        private FORMATETC _formatDescriptor;

        private byte[] _data;
        public override FORMATETC FormatDescriptor => _formatDescriptor;

        public override (nint, int) GetData()
        {
            var ptr = NatHelpers.ToHGLOBAL(_data);
            return (ptr, NatConstants.S_OK);
        }

        public byte[] RawData { get => _data; set => _data = value; }

        public RawDataDescriptor(short formatId, byte[] data)
        {
            _data = data;
            _formatDescriptor = new FORMATETC { cfFormat = formatId, dwAspect = DVASPECT.DVASPECT_CONTENT, lindex = -1, tymed = TYMED.TYMED_HGLOBAL};
        }

        public RawDataDescriptor(FORMATETC formatEtc, ref STGMEDIUM medium)
        {
            _formatDescriptor = formatEtc;
            if (medium.tymed == TYMED.TYMED_HGLOBAL)
            {
                var size = NatMethods.GlobalSize(medium.unionmember);

                _data = new byte[size];
                Marshal.Copy(medium.unionmember, _data, 0, (int)size);
            }
            else if (medium.tymed == TYMED.TYMED_ISTREAM)
            {
                var stm = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
                STATSTG stat = new();
                stm.Stat(out stat, 0);
                _data = new byte[stat.cbSize];
                stm.Read(_data, (int)stat.cbSize, nint.Zero);
                _formatDescriptor.tymed = TYMED.TYMED_HGLOBAL;
            }
            else
                throw new Exception("Unsupported format");
        }
    }


}
