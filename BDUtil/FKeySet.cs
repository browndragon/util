using System;

namespace BDUtil
{
    public class FKeySet<K, T> : KeySet<K, T>
    {
        readonly Func<T, K> KeyFunc;
        public FKeySet(Func<T, K> keyFunc) => KeyFunc = keyFunc;

        protected override K GetKey(T item) => KeyFunc(item);
    }
}