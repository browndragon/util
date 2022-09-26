using System;
using System.Linq.Expressions;
using BDUtil.Traces;

namespace BDUtil
{
    /// Convert between two types.
    public interface IConverter { }
    public interface IConverter<in TIn, out TOut> : IConverter
    {
        TOut Convert(TIn @in);
    }
    public static class Converter
    {
        public static TOut UpcastOrDefault<TIn, TOut>(TIn tin, TOut @null, TOut @default)
        => tin switch
        {
            null => @null,
            TOut tout => tout,
            _ => @default,
        };
        public static TOut UpcastOrDefault<TIn, TOut>(TIn tin, TOut @default)
        => UpcastOrDefault(tin, @default, @default);
        public static TOut UpcastOrDefault<TIn, TOut>(TIn tin)
        => UpcastOrDefault(tin, default(TOut));
    }
    /// Maps any type which supports am explicit or implicit conversion from TIn to TOut
    public class Converter<TIn, TOut> : IConverter<TIn, TOut>
    {
        public static readonly Converter<TIn, TOut> Default;
        readonly Func<TIn, TOut> Impl;
        public TOut Convert(TIn @in)
        {
            try { return Impl(@in); }
            catch { throw new NotSupportedException($"Can't convert {@in}:{typeof(TIn)} => :{typeof(TOut)} at runtime."); }
        }
        Converter(Func<TIn, TOut> impl) => Impl = impl;
        static Converter()
        {
            Type tin = typeof(TIn);
            Type tout = typeof(TOut);
            // Handle upcast-convertible TIn : TOut (esp. TIn == TOut).
            // I want to avoid a trip through object (which I'm not sure that the switch does, but it _could_...)
            // for e.g. `int<->int`.
            if (tout.IsAssignableFrom(tin))
            {
                Default = new(Converter.UpcastOrDefault<TIn, TOut>);
                return;
            }
            // Handle implicit or explicit cast operators:
            try
            {
                var source = Expression.Parameter(tin, "source");
                var convert = Expression.Convert(source, tout);
                Func<TIn, TOut> converted = Expression.Lambda<Func<TIn, TOut>>(convert, source).Compile();
                // Test it out, catch various widening etc flaws.
                // Checking data first ("is the method null?") is too complex to handle boxing/unboxing, type widening, etc.
                // This isn't ideal! It means that for instance if you want to support wrapperOfString->float,
                // you _must_ handle null safely (or else this `convert(default==(string)null)` will fail, and the type assumed bad).
                converted.Invoke(default);
                Default = new(converted);
            }
            // Handle inconvertibles.
            catch (Exception e)
            {
                e.DoTrace($"Suppressing inconvertible {tin}=>{tout}");
                Default = null;
            }
        }
    }
}