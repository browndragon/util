using System;

namespace BDUtil.Pubsub
{
    /// A generic type similar to Nullable.
    /// Mixin for unity types like ScriptableObject and MonoBehaviour to delegate their implementations
    /// to some concrete other type stored in Value.
    /// See Maybe for a concrete example.
    public interface IHas
    {
        bool HasValue { get; }
        object Value { get; }
    }
    public interface IHas<out T> : IHas
    {
        new T Value { get; }
    }
    /// Same idea, but the type is writeable.
    public interface IValue<T> : IHas<T> { new T Value { set; } }

    /// Same-ish idea, but it requires taking out a scope to get visibility into the data.
    /// Semantics are unclear: can multiple attempt an edit? Who knows.
    public interface IMutate<TRO, TRW> : IHas<TRO>, Scope.IScopable<TRW>
    {
        bool IsMutating { get; }
    }
}