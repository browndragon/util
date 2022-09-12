using System;
using BDUtil.Math;

namespace BDUtil
{
    /// Extensions & utilities for working with Func & Action.
    public static class Funcs
    {
        public static T GetTarget<T>(this Delegate thiz, T @default = default)
        => thiz?.Target switch
        {
            null => @default,
            T t => t,
            _ => @default,
        };

        /// Returns an action which calls Thiz only the first time it's invoked, none of the following.
        public static Action FirstCalling(this Action thiz)
        => () => { thiz?.Invoke(); thiz = null; };

        /// Returns a factory; thiz is called when the last action returned from the factory is invoked.
        public static Func<T, Action> LastCalling<T>(this Action thiz)
        {
            Lock count = default;
            Action Checkout(T t)
            {
                count++;
                bool hasRedeemed = false;
                void Redeem()
                {
                    hasRedeemed.AndThrow($"Duplicate unlock: {t}");
                    hasRedeemed = true;
                    if (!--count) thiz.OrThrow().Let(thiz = null).Invoke();
                }
                return Redeem;
            }
            return Checkout;
        }

        public static Action<float> GetLerpAction<T>(this IArith<T> thiz, T start, T end, Action<T> action)
        => (f) => action(thiz.Lerp(start, end, f));

        /// Transforms a function(input, prev)=> output into a pair (set(input),get()=>output=prev=func(input, prev)).
        /// Operation performed on Set (gets are nonmutating.)
        /// (this happens to return the setter & outparam the getter; see MakeGetter for the other of the pair).
        /// This also serves as a cache of the previous value.
        public static Action<TIn> MakeSetter<TIn, TOut>(this Func<TIn, TOut, TOut> thiz, out Func<TOut> getter, TOut initial = default)
        {
            TOut has = initial;
            getter = () => has;
            return (tin) => has = thiz(tin, has);
        }
        /// As MakeSetter but returns/outparams the other of the pair.
        public static Func<TOut> MakeGetter<TIn, TOut>(this Func<TIn, TOut, TOut> thiz, out Action<TIn> setter, TOut initial = default)
        {
            setter = thiz.MakeSetter(out var getter, initial);
            return getter;
        }
        /// As MakeSetter but the transforming function ignores the previous value.
        public static Action<TIn> MakeSetter<TIn, TOut>(this Func<TIn, TOut> thiz, out Func<TOut> getter, TOut initial = default)
        => ((Func<TIn, TOut, TOut>)((tin, _) => thiz(tin))).MakeSetter(out getter, initial);
        /// As MakeSetter but returns/outparams the other of the pair.
        public static Func<TOut> MakeGetter<TIn, TOut>(this Func<TIn, TOut> thiz, out Action<TIn> setter, TOut initial = default)
        {
            setter = thiz.MakeSetter(out var getter, initial);
            return getter;
        }
        public static Action MakeSetter(out Func<bool> getter)
        {
            bool hasCalled = false;
            getter = () => hasCalled;
            return () => hasCalled = true;
        }
        public static Func<bool> MakeGetter(out Action setter)
        {
            setter = MakeSetter(out Func<bool> getter);
            return getter;
        }
    }
}