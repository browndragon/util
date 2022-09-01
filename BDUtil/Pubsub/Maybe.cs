using System;
using System.Runtime.CompilerServices;

namespace BDUtil.Pubsub
{
    [Serializable]
    public readonly struct Maybe<T> : IHas<T>
    {
        public readonly Exception Exception;
        public readonly T Definitely;
        public Maybe(Exception exception, T definitely = default)
        {
            Exception = exception;
            Definitely = definitely;
        }
        public bool HasValue => Exception == null;
        public T Value => Exception != null ? throw Exception : Definitely;
        object IHas.Value => Value;
        public static implicit operator T(Maybe<T> thiz) => thiz.Value;
        public static implicit operator Maybe<T>(T value) => new(null, value);
    }

    /// Like a Nullable<T> that throws on access in the false case.
    public static class Maybe
    {
        public static Maybe<T> Of<T>(T value) => new(null, value);
        public static Maybe<T> Definite<T>(T value,
            [CallerFilePath] string callerPath = default,
            [CallerMemberName] string callerName = default,
            [CallerLineNumber] int lineNumber = default
        )
        => new(
            value == null
            ? new ArgumentNullException($"Null ({typeof(T)}) from {callerPath}/{callerName} L: {lineNumber}")
            : null,
            value
        );
    }
}