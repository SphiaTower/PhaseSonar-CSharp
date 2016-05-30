using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTIR {

    // The Pool class is the most important class in the object pool design pattern. It controls access to the
    // pooled objects, maintaining a list of available objects and a collection of objects that have already been
    // requested from the pool and are still in use. The pool also ensures that objects that have been released
    // are returned to a suitable state, ready for the next time they are requested. 
    public class Pool<T> {
        private List<T> _available = new List<T>();
        private List<T> _inUse = new List<T>();

        public T GetObject() {
            lock (_available) {
                if (_available.Count != 0) {
                    T po = _available[0];
                    _inUse.Add(po);
                    _available.RemoveAt(0);
                    return po;
                } else {
                    T po = newObject();
                    _inUse.Add(po);
                    return po;
                }
            }
        }

        private Object[] _args;

        private Pool(Object[] args) {
            _args = args;
        }

        private static Pool<T> sInstance;

        private static readonly String LOCK = "lock";
        public static Pool<T> GetInstance(Object[] args) {
            if (sInstance == null) {
                lock (LOCK){
                    if (sInstance == null) {
                        sInstance = new Pool<T>(args);
                    }
                }
            }
            return sInstance;
        }

        protected T newObject() {
            return (T)Activator.CreateInstance(typeof(T), _args);
        }

        public void ReleaseObject(T po) {
            CleanUp(po);

            lock (_available) {
                _available.Add(po);
                _inUse.Remove(po);
            }
        }

        private void CleanUp(T po) {
            // po.TempData = null;
        }
    }
}