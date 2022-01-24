using System.Collections.Generic;
using System.Threading;

namespace ModelDownloader.Utils
{
    // Implements most of IColleciton<T>
    public class ConcurrentHashSet<T>
    {
        private readonly ReaderWriterLock _lock = new ReaderWriterLock();
        private HashSet<T> _set = new HashSet<T>();

        private const int READER_TIMEOUT = 500;
        private const int WRITER_TIMEOUT = 1000;

        public int Count
        {
            get
            {
                _lock.AcquireReaderLock(READER_TIMEOUT);
                try
                {
                    return _set.Count;
                }
                finally
                {
                    if (_lock.IsReaderLockHeld)
                    {
                        _lock.ReleaseReaderLock();
                    }
                }
            }
        }

        public void Add(T item)
        {
            _lock.AcquireWriterLock(WRITER_TIMEOUT);
            try
            {
                _set.Add(item);
            }
            finally
            {
                if (_lock.IsWriterLockHeld)
                {
                    _lock.ReleaseWriterLock();
                }
            }
        }

        public void Clear()
        {
            _lock.AcquireWriterLock(WRITER_TIMEOUT);
            try
            {
                _set.Clear();
            }
            finally
            {
                if (_lock.IsWriterLockHeld)
                {
                    _lock.ReleaseWriterLock();
                }
            }
        }

        public bool Contains(T item)
        {
            _lock.AcquireReaderLock(READER_TIMEOUT);
            try
            {
                return _set.Contains(item);
            }
            finally
            {
                if (_lock.IsReaderLockHeld)
                {
                    _lock.ReleaseReaderLock();
                }
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.AcquireWriterLock(WRITER_TIMEOUT);
            try
            {
                _set.CopyTo(array, arrayIndex);
            }
            finally
            {
                if (_lock.IsWriterLockHeld)
                {
                    _lock.ReleaseWriterLock();
                }
            }
        }

        public bool Remove(T item)
        {
            _lock.AcquireReaderLock(READER_TIMEOUT);
            try
            {
                return _set.Remove(item);
            }
            finally
            {
                if (_lock.IsReaderLockHeld)
                {
                    _lock.ReleaseReaderLock();
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            _lock.AcquireReaderLock(READER_TIMEOUT);
            try
            {
                return _set.GetEnumerator();
            }
            finally
            {
                if (_lock.IsReaderLockHeld)
                {
                    _lock.ReleaseReaderLock();
                }
            }
        }
    }
}