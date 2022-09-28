using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Library
{
    // Like an enum element, dictionary key, etc.
    [CreateAssetMenu(menuName = "BDUtil/Key")]
    public class Key : ScriptableObject, IEquatable<Key>
    {
        public static Key Default => DefaultKey.main;

        public virtual bool Equals(Key other) => this == other;
        public override bool Equals(object obj) => obj is Key other && Equals(other);
        public override int GetHashCode() => base.GetHashCode();
    }
}