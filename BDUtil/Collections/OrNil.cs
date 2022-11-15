using System;
using System.Collections.Generic;
using BDUtil.Math;

namespace BDUtil
{
    // Replacement for non-serializable Nullable<T>==T? in structs.
    // BUT WAIT THERE'S MORE. For object-types, it provides an additional "nil" value.
    // This is so that
    [Serializable]
    public struct OrNil<T> : IHas<T>, IEquatable<OrNil<T>>
    {
        public bool HasValue;
        public T value;  // Public for unity serialization...
        public T Value
        {
            get => HasValue ? value : throw new ArgumentNullException("value");
            set
            {
                HasValue = true;
                this.value = value;
            }
        }
        object IHas.Value => Value;
        public OrNil(T value)
        {
            this = default;
            HasValue = true;
            this.value = value;
        }
        public static implicit operator OrNil<T>(T instance) => new(instance);
        public static bool operator ==(OrNil<T> a, OrNil<T> b) => a.Equals(b);
        public static bool operator !=(OrNil<T> a, OrNil<T> b) => !a.Equals(b);

        public bool Equals(OrNil<T> other)
        {
            if (HasValue ^ other.HasValue) return false;
            if (!HasValue) return true;
            return Value.Equals(other.Value);
        }
        public override bool Equals(object other) => other is OrNil<T> orNil && Equals(orNil);
        public override int GetHashCode() => Chain.Hash ^ this.GetValueOrDefault();
        public override string ToString() => HasValue ? $"Some({value})" : $"Nil<{typeof(T)}>";
    }
    public static class OrNil
    {
        public static bool TryGetValue<T>(in this OrNil<T> thiz, out T value)
        {
            value = thiz.HasValue ? thiz.Value : default;
            return thiz.HasValue;
        }
        public static T GetValueOrDefault<T>(in this OrNil<T> thiz, T @default = default)
        {
            if (thiz.TryGetValue(out T value)) return value;
            return @default;
        }
        public static T? GetNullable<T>(in this OrNil<T> thiz)
        where T : struct
        => thiz.HasValue ? thiz.Value : null;
        public static OrNil<T> OfNullable<T>(T? t)
        where T : struct
        => t switch
        {
            null => new(),
            T t2 => new(t2),
        };

        public static TOut Switch<TIn, TOut>(in this OrNil<TIn> thiz, Func<TIn, TOut> some, Func<TOut> none)
        => thiz.HasValue ? some(thiz.Value) : none();
    }
}