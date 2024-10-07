using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace VirtualFiles
{
    /* An IStream returning no actual data (effectively, returns zeroes or garbage);
     * Made for testing purposes only
     */
    public class DummyStream(Int64 size) : IStream
    {
        private Int64 _remains = size;
        void IStream.Clone(out IStream ppstm)
        {
            throw new NotImplementedException();
        }

        void IStream.Commit(int grfCommitFlags)
        {
            throw new NotImplementedException();
        }

        void IStream.CopyTo(IStream pstm, long cb, nint pcbRead, nint pcbWritten)
        {
            throw new NotImplementedException();
        }

        void IStream.LockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        void IStream.Read(byte[] pv, int cb, nint pcbRead)
        {
            int to_write = _remains > cb ? cb : (int)_remains;
            Marshal.WriteInt32(pcbRead, to_write);
            _remains -= to_write;
        }

        void IStream.Revert()
        {
            throw new NotImplementedException();
        }

        void IStream.Seek(long dlibMove, int dwOrigin, nint plibNewPosition)
        {
            throw new NotImplementedException();
        }

        void IStream.SetSize(long libNewSize)
        {
            throw new NotImplementedException();
        }

        void IStream.Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            throw new NotImplementedException();
        }

        void IStream.UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            throw new NotImplementedException();
        }

        void IStream.Write(byte[] pv, int cb, nint pcbWritten)
        {
            throw new NotImplementedException();
        }
    }
}
