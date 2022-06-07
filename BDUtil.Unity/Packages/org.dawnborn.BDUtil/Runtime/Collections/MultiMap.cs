using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    [Serializable]
    public sealed class MultiMap<K, V> : Raw.Map<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        [SuppressMessage("IDE", "IDE0044")]
        Proxy<KeyValuePair<K, V>, Pair<K, V>> Proxy = new(Pair.ToPair, Pair.ToKVP);

        void ISerializationCallbackReceiver.OnAfterDeserialize() => Proxy.After(this);
        void ISerializationCallbackReceiver.OnBeforeSerialize() => Proxy.Before(this);
    }
}