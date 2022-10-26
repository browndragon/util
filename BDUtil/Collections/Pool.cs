using System.Collections.Generic;

namespace BDUtil.Raw
{
    public abstract class Pool<T>
    {
        readonly List<T> Deque = new();
        public T Acquire()
        {
            if (!Deque.PopFrom(Deques.Ends.Back, out T data)) data = OnCreate();
            OnAcquire(data);
            return data;
        }
        public void Release(T t) { if (OnRelease(t)) Deque.Add(t); }

        protected abstract T OnCreate();
        protected abstract void OnAcquire(T t);
        protected abstract bool OnRelease(T t);
    }
}
