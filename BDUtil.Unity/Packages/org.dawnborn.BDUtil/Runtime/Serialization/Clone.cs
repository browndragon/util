using System;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// A prefab with a Clone component exposes its ancestor prefab asset.
    /// There's an automatic tool which ensures that a prefab has its own Ref set to itself; see CloneLinker.
    [DisallowMultipleComponent]
    public class Clone : MonoBehaviour
    {
        public Ref<Clone> PrefabRef;  // The ancestor-pointer for the prefab root from which this clone derived.
        public Ref<T> GetRootRef<T>() where T : Component => (Ref<T>)PrefabRef.Load().GetComponent<T>();
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

        public static Clone Acquire(Ref<Clone> @ref, int limit = int.MaxValue)
        => (Pools.TryGetValue(@ref, out Pool pool)
        ? pool
        : (Pools[@ref] = new(@ref.Load())))
        .Acquire(limit);
        public static T Acquire<T>(Ref<T> @ref, int limit = int.MaxValue)
        where T : Component
        => Acquire((Ref<Clone>)@ref.Load().GetComponent<Clone>()).GetComponent<T>();

        public static void Release(Clone clone) => Pools[clone.PrefabRef].Release(clone);
        public static void Release<T>(T clone)
        where T : Component
        => Release(clone.GetComponent<Clone>());

        public static void Clear(Ref<Clone> @ref)
        { if (Pools.TryGetValue(@ref, out Pool pool)) pool.Clear(); }

        public static void ClearAll()
        {
            foreach (Pool pool in Pools.Values) pool.Clear();
            Pools.Clear();
        }

        static readonly Dictionary<Ref<Clone>, Pool> Pools = new();
        internal class Pool : Scopes.IScopable<Clone>
        {
            public int Outstanding { get; private set; }
            readonly Clone Proto;
            readonly Deque<Clone> Cache = new();
            public Pool(Clone proto) => Proto = proto.Root;
            public void Clear()
            {
                foreach (Clone clone in Cache) Destroy(clone.gameObject);
                Cache.Clear();
                Outstanding = 0;
            }
            public Clone Acquire() => Acquire(int.MaxValue);
            public Clone Acquire(int limit)
            {
                if (!Cache.PopFront(out Clone pooled))
                {
                    if (Outstanding >= limit) return null;
                    pooled = New(Proto);
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
    }
}