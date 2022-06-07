using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{

    [Serializable]
    public sealed class Set<T> : Raw.Set<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        [SuppressMessage("IDE", "IDE0044")]
        Proxy<T> Proxy = new();

        void ISerializationCallbackReceiver.OnAfterDeserialize() => Proxy.After(this);
        void ISerializationCallbackReceiver.OnBeforeSerialize() => Proxy.Before(this);
    }
}