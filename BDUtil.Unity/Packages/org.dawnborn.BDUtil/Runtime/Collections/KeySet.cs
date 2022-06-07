using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    // Omitted, because it's not actually serializable (you need to extend it).
    // Unity serialization & property drawers play badly with subclasses.
    // See FKeySet for one which is serializable, using delegates.
    //
    // [Serializable]
    // public abstract class KeySet<K, T> : Raw.KeySet<K, T>, ISerializationCallbackReceiver
    // {
    //     [SerializeField]
    //     [SuppressMessage("IDE", "IDE0044")]
    //     Proxy<T> Proxy = new();

    //     void ISerializationCallbackReceiver.OnAfterDeserialize() => Proxy.After(this);
    //     void ISerializationCallbackReceiver.OnBeforeSerialize() => Proxy.Before(this);
    // }
}