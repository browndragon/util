using System;
using System.Collections.Generic;

namespace BDUtil
{
    [Serializable]
    public sealed class Set<T> : Store<HashSet<T>, T, T>
    {
        protected override T Internal(T t) => t;
        protected override T External(T t) => t;
    }
}
