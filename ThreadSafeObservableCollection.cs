using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;


    /// <summary>
    /// a thread safe observable collection using reader writer lock - created with the help of http://web.archive.org/web/20101105144104/http://www.deanchalk.me.uk/post/Thread-Safe-Dispatcher-Safe-Observable-Collection-for-WPF.aspx
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
    {
        private CoreDispatcher _dispatcher;
        internal ReaderWriterLockSlim sync = new System.Threading.ReaderWriterLockSlim();

        public ThreadSafeObservableCollection(CoreDispatcher dispatcher,IEnumerable<T> collection=null)
        {
            //copy the collection to ourself
            if (collection!=null)
            {
                foreach (var item in collection)
                {
                    base.Add(item);
                }
            }
            _dispatcher = dispatcher;
        }

        public new void Add(T item)
        {
            if (_dispatcher.HasThreadAccess)
                DoAdd(item);
            else
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => DoAdd(item));
        }

        private void DoAdd(T item)
        {
            sync.EnterWriteLock();
            base.Add(item);
            sync.ExitWriteLock();
        }

        public void Clear()
        {
            if (_dispatcher.HasThreadAccess)
                DoClear();
            else
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal,  DoClear);
        }

        private void DoClear()
        {
            sync.EnterWriteLock();
            base.Clear();
            sync.ExitWriteLock();
        }

        public bool Contains(T item)
        {
            sync.EnterReadLock();
            var result = base.Contains(item);
            sync.ExitReadLock();
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            sync.EnterWriteLock();
            base.CopyTo(array,arrayIndex);
            sync.ExitWriteLock();
        }

        public int Count
        {
            get
            {
                sync.EnterReadLock();
                var result = base.Count;
                sync.ExitReadLock();
                return result;
            }
        }

      

        
        public bool Remove(T item)
        {
            if (_dispatcher.HasThreadAccess)
                return DoRemove(item);
            else
            {
                bool? op=null;
                var removeTask = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        op = DoRemove(item);
                    });
                removeTask.AsTask().Wait();
                if (op == null )
                    return false;
                return op.Value;
            }
        }

        private bool DoRemove(T item)
        {
            sync.EnterWriteLock();
            var result=base.Remove(item);
            sync.ExitWriteLock();
            return result;

        }

        
        public int IndexOf(T item)
        {
            sync.EnterReadLock();
            var result = base.IndexOf(item);
            sync.ExitReadLock();
            return result;

           
        }

        public void Insert(int index, T item)
        {
            if (_dispatcher.HasThreadAccess)
                DoInsert(index, item);
            else
            {

                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DoInsert(index, item));
            }
        }

        private void DoInsert(int index, T item)
        {
            sync.EnterWriteLock();
          
            base.Insert(index,item);
            sync.ExitWriteLock();
        }

        public void RemoveAt(int index)
        {
            if (_dispatcher.HasThreadAccess)
                DoRemoveAt(index);
            else
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => DoRemoveAt(index));
        }

        private void DoRemoveAt(int index)
        {
            sync.EnterWriteLock();
            if (base.Count == 0 || base.Count <= index)
            {
                sync.ExitWriteLock();
                return;
            }
          
            base.RemoveAt(index);
            sync.ExitWriteLock();

        }

        public T this[int index]
        {
            get
            {
                sync.EnterReadLock();
                var result = base[index];
                sync.ExitReadLock();
                return result;
            }
            set
            {
                sync.EnterWriteLock();
                if (base.Count == 0 || base.Count <= index)
                {
                    sync.ExitWriteLock();
                    return;
                }
                base[index] = value;
                sync.ExitWriteLock();
            }

        }

        public IEnumerable<T> AsLocked()
        {
            return new ThreadSafeObservableCollectionEnumerableWrapper<T>(this);
        }
    }

