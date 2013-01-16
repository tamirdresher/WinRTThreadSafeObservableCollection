using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


    /// <summary>
    /// Created with the help of http://www.codeproject.com/Articles/56575/Thread-safe-enumeration-in-C 
    /// provides a ThreadSafe enumerator for ThreadSafeObservableCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SafeReaderWriterEnumerator<T> : IEnumerator<T>
    {
        // this is the (thread-unsafe)
        // enumerator of the underlying collection
        private readonly IEnumerator<T> m_Inner;
        // this is the object we shall lock on. 
        private ReaderWriterLockSlim m_lock;

        public SafeReaderWriterEnumerator(ThreadSafeObservableCollection<T> inner, ReaderWriterLockSlim @lock)
        { 
            m_lock = @lock;
            // entering lock in constructor
            m_lock.EnterReadLock();
            m_Inner = inner.GetEnumerator();
           
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            // .. and exiting lock on Dispose()
            // This will be called when foreach loop finishes
            m_lock.ExitReadLock();
            m_Inner.Dispose();
        }

        #endregion

        #region Implementation of IEnumerator

        // we just delegate actual implementation
        // to the inner enumerator, that actually iterates
        // over some collection

        public bool MoveNext()
        {
            return m_Inner.MoveNext();
        }

        public void Reset()
        {
            m_Inner.Reset();
        }

        public T Current { get { return m_Inner.Current; }  }

       
        object IEnumerator.Current
        {
            get { return m_Inner.Current; }
        }

        #endregion
    }

