using System;
using UnityEngine;

namespace BDUtil.Pooling
{

    /// A prefab with a Clone component exposes an operation that lets you create child clones which refer back to
    /// the parent; this lets them use
    public class Clone : MonoBehaviour
    {
        [field: SerializeField, Tooltip("Maximum pooled elements to allow outstanding")]
        public int PoolLimit { get; private set; }
        /// Root prefab to maintain links to/from.
        [SerializeField] Clone root = null;
        public Clone Root => root ?? this;
        public bool IsClone => root != null;
        public Clone NewClone()
        {
            if (root != null) return root.NewClone();
            var ret = EditorUtils.CloneInactive(gameObject).GetComponent<Clone>();
            ret.root = this;
            return ret;
        }
    }
    /// A little bit nicer wrt the unity interface: you can take a Clone<MyComponent> and have _some_ confidence you've got both attached.
    [Serializable]
    public struct Clone<T>
    where T : Component
    {
        public T Component;
        public Clone Root => Component?.GetComponent<Clone>().Root;
        public static implicit operator Clone(Clone<T> thiz) => thiz.Component?.GetComponent<Clone>();
        public static implicit operator Clone<T>(Clone thiz) => new() { Component = thiz?.GetComponent<T>() };
    }
}