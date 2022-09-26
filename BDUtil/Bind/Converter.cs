using System;
using System.Linq.Expressions;
using System.Reflection;
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
        // Checks that two methods have the same shape.
        internal static void CheckInvokeMethodInfos(Type tin, Type tout)
        {
            MethodInfo min = tin.GetMethod("Invoke"), mout = tout.GetMethod("Invoke");
            if (min == null || mout == null) throw new ArgumentException($"{tin}.{min} or {tout}.{mout} no Invoke method!");
            if (min.ReturnType == null && mout.ReturnType != null) throw new ArgumentException($"{tin}#ret.{min.ReturnType} !<- {tout}#ret.{mout.ReturnType}");
            ParameterInfo[] pin = min.GetParameters();
            ParameterInfo[] pout = mout.GetParameters();
            if (pin.Length != pout.Length) throw new ArgumentException($"{tin}.Length != {tout}.Length");
            for (int i = 0; i < pin.Length; ++i)
            {
                if (!pout[i].ParameterType.IsAssignableFrom(pin[i].ParameterType)) throw new ArgumentException($"{tin}(#{i}.{pin[i].ParameterType}) !<- {tout}(#{i}.{pout[i].ParameterType})");
            }
            return;
        }
        // Converts delegate A to B -- you're responsible for _everything_, and for impl reasons,
        // it doesn't even check that they're delegates!!!
        internal static TOut CrosscastDelegate<TIn, TOut>(TIn tin)
        => (TOut)(object)Delegate.CreateDelegate(
            typeof(TOut),
            ((Delegate)(object)tin).Target,
            ((Delegate)(object)tin).Method
        );

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
            try
            {
                // This is awful, but necessary.
                // Handle delegates by casting to a consistent alternate representation.
                // Doesn't work if they're ACTUALLY multicast!
                // This doesn't need to handle Cast<SomeAction, LiterallyADelegate> because those are cast convertible, above.
                Type delType = typeof(Delegate);
                if (delType.IsAssignableFrom(tin) && delType.IsAssignableFrom(tout))
                {
                    Converter.CheckInvokeMethodInfos(tin, tout);
                    Default = new(Converter.CrosscastDelegate<TIn, TOut>);
                    return;
                }
                // Handle implicit or explicit cast operators:
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
            // Handle inconvertibles by returning null.
            catch (Exception e)
            {
                e.DoTrace($"Suppressing inconvertible {tin}=>{tout}:\nBecause: {e}");
                Default = null;
            }
        }
    }
}