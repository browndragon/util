using System;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pooling
{
    /// A specific
    public class Pool : Scopes.IScopable<Clone>
    {
        public int Outstanding { get; private set; }
        readonly Clone Root;
        readonly Deque<Clone> Cache = new();
        public Pool(Clone root) => Root = root.Root;
        public Clone Acquire()
        {
            if (!Cache.PopFront(out Clone pooled))
            {
                if (Outstanding >= Root.PoolLimit) return null;
                pooled = Root.NewClone();
            }
            pooled.gameObject.SetActive(true);
            Outstanding++;
            return pooled;
        }
        public void Release(Clone released)
        {
            if (released == null) return;
            released.gameObject.SetActive(false);
            Outstanding--;
            Cache.PushBack(released);
        }
    }
    public class Pools : Scopes.IScopable<Clone, Clone>
    {
        public static readonly Pools main = new();
        readonly Dictionary<Clone, Pool> Cache = new();
        public Clone Acquire(Clone root)
        {
            root = root.Root.OrThrow();
            if (!Cache.TryGetValue(root, out Pool pool)) Cache[root] = pool = new(root);
            return pool.Acquire();
        }
        public void Release(Clone root, Clone instance) => Cache[root].Release(instance);
    }
}