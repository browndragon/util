using System;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// A prefab with a Clone component exposes its ancestor prefab asset.
    /// There's an automatic tool which ensures that a prefab has its own Ref set to itself; see CloneLinker.
    /// This automatically gets you pooling behaviour as well.
    [DisallowMultipleComponent]
    public class Clone : MonoBehaviour
    {
        public Ref<Clone> PrefabRef;  // The ancestor-pointer for the prefab root from which this clone derived.
        public Clone Root => IsClone ? PrefabRef.Load() : this;
        public bool IsClone => gameObject.scene.IsValid();

        public static T New<T>(T proto)
        where T : UnityEngine.Object
        => proto switch
        {
            null => null,
            Clone c => (T)(object)(c.IsClone ? New(c.Root) : c.NewClone()),
            Component c => New(c.GetComponent<Clone>() ?? throw new NotSupportedException($"Can't clone uncloneable {proto}")).GetComponent<T>(),
            GameObject g => (T /*==GameObject...*/)(object)New(g.GetComponent<Clone>() ?? throw new NotSupportedException($"Can't clone uncloneable {proto}")).gameObject,
            _ => throw new NotSupportedException($"Can't clone uncloneable {proto}"),
        };
        public Clone NewClone()
        {
            IsClone.AndThrow();
            var ret = EditorUtils.CloneInactive(gameObject).GetComponent<Clone>();
            ret.PrefabRef = PrefabRef;
            return ret;
        }

        public static Clone Acquire(Ref<Clone> @ref)
        => (Pools.TryGetValue(@ref, out Pool pool)
        ? pool
        : (Pools[@ref] = new(@ref.Load())))
        .Acquire();
        public static T Acquire<T>(Ref<T> @ref)
        where T : Component
        => Acquire((Ref<Clone>)@ref.Load().GetComponent<Clone>()).GetComponent<T>();

        // If there were already members in the scene...
        public static void Release(Clone clone) => (Pools.TryGetValue(clone.PrefabRef, out Pool pool)
        ? pool
        : (Pools[clone.PrefabRef] = new(clone.Root)))
        .Release(clone);
        public static void Release<T>(T clone)
        where T : Component
        => Release(clone.GetComponent<Clone>());

        public static void Clear(Ref<Clone> @ref)
        { if (Pools.TryGetValue(@ref, out Pool pool)) pool.Clear(); }

        public static void SetLimit(Ref<Clone> @ref, int limit = int.MaxValue)
        {
            if (!Pools.TryGetValue(@ref, out Pool pool)) pool = Pools[@ref] = new(@ref.Load());
            pool.Limit = limit;
            while (pool.Cache.Count + pool.Outstanding > limit
            && pool.Cache.PopBack(out Clone clone)) Destroy(clone.gameObject);
        }

        public static void ClearAll()
        {
            foreach (Pool pool in Pools.Values) pool.Clear();
            Pools.Clear();
        }

        static readonly Dictionary<Ref<Clone>, Pool> Pools = new();
        internal class Pool : Scopes.IScopable<Clone>
        {
            public int Outstanding { get; private set; }
            public int Limit = int.MaxValue;
            readonly Clone Proto;
            readonly internal Deque<Clone> Cache = new();
            public Pool(Clone proto) => Proto = proto.Root;
            public void Clear()
            {
                foreach (Clone clone in Cache) Destroy(clone.gameObject);
                Cache.Clear();
                Outstanding = 0;
            }
            public Clone Acquire()
            {
                if (!Cache.PopFront(out Clone pooled))
                {
                    if (Outstanding >= Limit) return null;
                    pooled = New(Proto);
                }
                pooled.gameObject.SetActive(true);
                Outstanding++;
                return pooled;
            }
            public void Release(Clone released)
            {
                if (released == null) return;
                Outstanding--;
                if (Cache.Count > Limit)
                {
                    Destroy(released.gameObject);
                }
                else
                {
                    released.gameObject.SetActive(false);
                    Cache.PushBack(released);
                }
            }
        }
    }
}