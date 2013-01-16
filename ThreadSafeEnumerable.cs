using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class ThreadSafeObservableCollectionEnumerableWrapper<T> : IEnumerable<T>
    {
        private readonly ThreadSafeObservableCollection<T> m_Inner;


        public ThreadSafeObservableCollectionEnumerableWrapper(ThreadSafeObservableCollection<T> observable)
        {
            
            m_Inner = observable;
        }

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return new SafeReaderWriterEnumerator<T>(m_Inner, m_Inner.sync);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

