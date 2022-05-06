using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    [Serializable]
    public sealed class Table<R, C, V> : Raw.Table<R, C, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        [SuppressMessage("IDE", "IDE0044")]
        Proxy<KeyValuePair<(R, C), V>, Pair<Pair<R, C>, V>> Proxy = new(Pair.ToNestPair, Pair.ToTupleKVP);

        void ISerializationCallbackReceiver.OnAfterDeserialize() => Proxy.After(this);
        void ISerializationCallbackReceiver.OnBeforeSerialize() => Proxy.Before(this);
    }
}