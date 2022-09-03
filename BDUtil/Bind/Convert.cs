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

    /// Maps any type which supports am explicit or implicit conversion from TIn to TOut
    public class Converter<TIn, TOut> : IConverter<TIn, TOut>
    {
        public static readonly Converter<TIn, TOut> Default;
        readonly Func<TIn, TOut> Impl;
        public TOut Convert(TIn @in)
        {
            try
            {
                return Impl(@in);
            }
            catch
            {
                throw new NotSupportedException($"Can't convert {@in}:{typeof(TIn)} => :{typeof(TOut)} and didn't learn early.");
            }
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
                // Throws if nosuch conversion:
                var method = convert.Method;
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