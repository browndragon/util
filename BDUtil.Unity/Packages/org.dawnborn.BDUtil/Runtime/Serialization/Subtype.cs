

using System;
using UnityEngine;

namespace BDUtil
{
    /// Marks an *also `SerializeRef`* field as wanting subtype selection.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SubtypeAttribute : PropertyAttribute
    {
        /// The preferred (& initial) value to select; null for (well) null
        public Type Default = null;
        public bool? Serializable = true;
        public bool? Instantiable = true;
        public bool? Unity = false;
        public bool PrintDebug;
        public SubtypeAttribute() { }
    }

    /// A serialized-Type field whose value Types are restricted to those `assignableFrom(typeof(T))`.
    [Serializable]
    public struct Subtype<T> : ISerializationCallbackReceiver
    {
        public static readonly Subtype<T> Default = new(typeof(T));
        internal static readonly Type Restrict = typeof(T);
        Type type;
        public Type Type
        {
            get => invalid ? null : type;
            set
            {
                /// Since we use `Subtype<>` in our Drawer, we need to accept null T...
                invalid = value == null || !(Restrict?.IsAssignableFrom(value) ?? false);
                type = value;
            }
        }
        public Type RawType => type;
        bool invalid;
        [SerializeField] string serialized;

        public Subtype(Type type)
        {
            this = default;
            Type = type;
        }
        public Subtype(Subtype<T> type) : this(type.Type) { }

        public bool IsAssignableFrom<T2>(Subtype<T2> other) where T2 : T
        => !other.invalid && IsAssignableFrom(other.Type);
        public bool IsAssignableFrom(Type other)
        => Type?.IsAssignableFrom(other) ?? false;

        public bool IsInstanceOfType(T instance)
        => Type?.IsInstanceOfType(instance) ?? false;

        public T CreateInstance()
        {
            Type type = Type;
            if (type == null) return default;
            return (T)Activator.CreateInstance(type);
        }

        public static implicit operator bool(Subtype<T> thiz) => !thiz.invalid;
        public static implicit operator Type(Subtype<T> subtype) => subtype.Type;
        public static implicit operator Subtype<T>(Type type) => new(type);
        public static implicit operator Subtype<object>(Subtype<T> subtype) => new(subtype.type);
        public static implicit operator Subtype<T>(Subtype<object> supertype) => new(supertype.type);

        void ISerializationCallbackReceiver.OnBeforeSerialize() => serialized = type?.AssemblyQualifiedName;
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Type = (serialized == null || serialized.Length == 0) ? null : Type.GetType(serialized);
            serialized = null;
        }
    }
}