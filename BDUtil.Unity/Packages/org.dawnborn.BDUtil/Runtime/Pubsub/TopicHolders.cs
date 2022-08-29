using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Pubsub
{
    /// A base monobehaviour topic holder: clear the topic on enable/disable.
    public abstract class MonoBehaviour<T> : MBHolder<T>
    where T : class, ITopic, new()
    {
        public MonoBehaviour() => Value = new();
        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable() => Value?.Clear();
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() => Value?.Clear();
    }
    /// A base scriptableobject topic holder: clear the topic on enable/disable.
    public abstract class ScriptableObject<T> : SOHolder<T>
    where T : class, ITopic, new()
    {
        public ScriptableObject() => Value = new();
        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable() => Value?.Clear();
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() => Value?.Clear();
    }
}