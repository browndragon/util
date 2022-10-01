using System;
using System.Collections.Generic;
using BDUtil.Fluent;

namespace BDUtil
{
    public interface IHas
    {
        object Value { get; }
    }
    public interface ICanHas : IHas
    {
        bool HasValue { get; }
    }
    public interface IHas<out T> : IHas
    {
        new T Value { get; }
    }
    public interface ISet
    {
        object Value { set; }
    }
    public interface ISet<T> : ISet
    {
        new T Value { set; }
    }
    public interface ICanHas<out T> : ICanHas, IHas<T> { }

    public static class Has
    {
        public static CanHas<T> None<T>() => new(false, default);
        public static CanHas<T> Some<T>(T t) => new(true, t.OrThrow());
        public static CanHas<T> Any<T>(T t) => new(t == null, t);
        public static bool TryGetValue(this ICanHas thiz, out object into) => thiz.HasValue.Let(into = thiz.Value);
        public static bool TryGetValue<T>(this ICanHas<T> thiz, out T into) => thiz.HasValue.Let(into = thiz.Value);
        public static object GetValueOrDefault(this ICanHas thiz, object @default = default) => thiz.TryGetValue(out var o) ? o : @default;
        public static T GetValueOrDefault<T>(this ICanHas<T> thiz, T @default = default) => thiz.TryGetValue(out var o) ? o : @default;
    }
    public readonly struct CanHas<T> : ICanHas<T>, IEquatable<CanHas<T>>, IComparable<CanHas<T>>
    {
        static readonly IEqualityComparer<T> Eq = EqualityComparer<T>.Default;
        static readonly IComparer<T> Cmp = Comparer<T>.Default;
        public bool HasValue { get; }
        public T Value { get; }
        object IHas.Value => Value;
        public CanHas(bool has, T value) { HasValue = has; Value = value; }
        public void Deconstruct(out T value)
        {
            HasValue.OrThrowInternal();
            value = Value;
        }
        public bool Equals(CanHas<T> other) => HasValue == other.HasValue && (!HasValue || Eq.Equals(Value, other.Value));
        public int CompareTo(CanHas<T> other) => (HasValue, other.HasValue) switch
        {
            (true, true) => Cmp.Compare(Value, other.Value),
            (false, true) => -1,
            (true, false) => +1,
            (false, false) => 0,
        };
        public override bool Equals(object obj) => obj switch
        {
            null => !HasValue,
            CanHas<T> other => Equals(other),
            _ => false,
        };
        public override int GetHashCode() => HasValue ? Value?.GetHashCode() ?? 0 : 0;
        public static implicit operator bool(CanHas<T> thiz) => thiz.HasValue;
    }
}
