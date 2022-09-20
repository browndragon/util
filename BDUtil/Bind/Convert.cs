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
        public static bool IsImplicitlyConverted(Type t)
        {
            if (t == null) return false;
            if (t.IsEnum) return true;
            if (t.IsPrimitive) return true;
            return false;
        }
        public static bool IsImplicitlyConverted(Type tin, Type tout)
        {
            if (!IsImplicitlyConverted(tin)) return false;
            if (!IsImplicitlyConverted(tout)) return false;
            return true;
        }
    }
    /// Maps any type which supports am explicit or implicit conversion from TIn to TOut
    public class Converter<TIn, TOut> : IConverter<TIn, TOut>
    {
        public static readonly Converter<TIn, TOut> Default;
        readonly Func<TIn, TOut> Impl;
        public TOut Convert(TIn @in)
        {
            try { return Impl(@in); }
            catch { throw new NotSupportedException($"Can't convert {@in}:{typeof(TIn)} => :{typeof(TOut)} and didn't learn early."); }
        }
        Converter(Func<TIn, TOut> impl) => Impl = impl;
        static Converter()
        {
            Type tin = typeof(TIn);
            Type tout = typeof(TOut);
            try
            {
                var source = Expression.Parameter(tin, "source");
                var convert = Expression.Convert(source, tout);
                if (convert.Method == null && !Converter.IsImplicitlyConverted(tin, tout))
                {
                    throw new NotSupportedException($"Can't convert {tin}->{tout} and found out early");
                }
                Func<TIn, TOut> converted = Expression.Lambda<Func<TIn, TOut>>(convert, source).Compile();
                Default = new(converted);
            }
            catch (Exception e)
            {
                e.DoTrace("Suppressing inconvertible");
                Default = null;
            }
        }
    }
}