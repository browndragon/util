using System;
using UnityEngine;

namespace BDUtil
{
    /// Specifically an inlined Ref<IHolder<T>>, which is otherwise kinda messy as a client.
    [Serializable]
    public struct Holder<T> : IHolder<T>
    {
        [SerializeField] Ref<IHolder<T>> Data;
        public T Value => Data.Value == default ? default : Data.Value.Value;
        public static implicit operator T(Holder<T> thiz) => thiz.Value;
    }

    /// A monobehaviour whose purpose is to provide access to some other object.
    /// This is abstract since inspector will need named subclasses.
    public abstract class MBHolder<T> : MonoBehaviour, IHolder<T>
    { public virtual T Value { get; protected set; } }

    /// A scriptableobject whose purpose is to provide access to some other object.
    /// This is abstract since inspector will need named subclasses.
    public abstract class SOHolder<T> : ScriptableObject, IHolder<T>
    { public virtual T Value { get; protected set; } }
}