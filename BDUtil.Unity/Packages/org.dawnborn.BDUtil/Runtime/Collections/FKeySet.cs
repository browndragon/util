using System;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    [Serializable]
    public sealed class FKeySet<K, T> : Raw.FKeySet<K, T>, ISerializationCallbackReceiver
    {
        public FKeySet(Func<T, K> keyFunc) : base(keyFunc) { }

        [SerializeField]
        [SuppressMessage("IDE", "IDE0044")]
        Proxy<T> Proxy = new();

        void ISerializationCallbackReceiver.OnAfterDeserialize() => Proxy.After(this);
        void ISerializationCallbackReceiver.OnBeforeSerialize() => Proxy.Before(this);

    }
}