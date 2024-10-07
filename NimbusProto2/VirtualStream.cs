
using System.Runtime.InteropServices;
using IStream = System.Runtime.InteropServices.ComTypes.IStream;

namespace NimbusProto2
{
    /* A virtual IStream with a 'push' and a 'pull' ends;
     * The client (e.g. Windows Explorer) waits on the 'pull' end by calling Read(), 
     * while our application runs and asynchronous task, reading an HTTP stream
     * and forwarding received chunks of data into the 'push' end of the stream.
     * 
     * Limitation: no concurrent read access, at most one thread at a time may
     * call Read() 
     */
    internal class VirtualStream : IStream
    {
        private Queue<byte[]?> _blockQueue = [];

        private byte[]? _currentBlock;
        private int _position = 0; // position in the current block
        private int _error = 0;

        // called on the 'push' thread
        // pushing null means end-of-stream
        public void Push(byte[]? block)
        {
            lock(_blockQueue)
            {
                _blockQueue.Enqueue(block);
                Monitor.Pulse(_blockQueue);
            }
        }

        public void SignalError()
        {
            lock(_blockQueue)
            {
                _error = -2147467259; // HRESULT E_FAIL
                Monitor.Pulse(_blockQueue);
            }
        }

        // called on the 'pull' thread
        private void PullIfNeeded()
        {
            // returns immediately if there's data available in _currentBlock;
            // otherwise waits for the client to push some data;
            // guarantees that there's data present when this function is done,
            // or at least there's an error condition reported
            if (null != _currentBlock && _position < _currentBlock.Length)
                return;
            
            _currentBlock = null;

            lock(_blockQueue)
            {
                while (true)
                {
                    if(_error != 0 || _blockQueue.TryDequeue(out _currentBlock))
                    {
                        if(_error != 0)
                            Marshal.ThrowExceptionForHR(_error);
                        _position = 0;
                        return;
                    }
                    else Monitor.Wait(_blockQueue);
                }
            }
        }
        void IStream.Read(byte[] pv, int cb, nint pcbRead)
        {
            PullIfNeeded();

            Marshal.WriteInt32(pcbRead, 0);

            if(_currentBlock?.Length == 0 || _error != 0)
                return;

            int remains = _currentBlock!.Length - _position;
            int to_read = Math.Min(remains, cb);
            
            Array.Copy(_currentBlock, _position, pv, 0, to_read);
            Marshal.WriteInt32(pcbRead, to_read);

            _position += to_read;
        }

        #region IStream non-implementation
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

        void IStream.Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
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
        #endregion

    }
}
