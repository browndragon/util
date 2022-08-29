using System;
using UnityEngine;

namespace BDUtil
{
    /// Holds a fully generic reference to an interface-implementing UE.Object.
    /// Theoretically this could be extended to _also_ support C# interface instances.
    /// But when I saw it, I didn't like it.
    [Serializable]
    public struct Ref<T> : IHolder<T>
    {
        /// Keep in sync with .Editor.RefDrawer
        [SerializeField, /* Subtype(typeof(T)) // Not legal in C#! */] UnityEngine.Object Data;
        public T Value => Data switch
        {
            null => default,
            T t => t,
            _ => default,
        };

        public static implicit operator T(Ref<T> thiz) => thiz.Value;
        public static implicit operator Ref<T>(T thiz) => thiz switch
        {
            null => new(),
            // https://github.com/dotnet/roslyn/issues/54193
            UnityEngine.Object data => new Ref<T>() { Data = data },
            _ => new(),
        };
    }
}