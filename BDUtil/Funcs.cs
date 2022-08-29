using System;
using System.Collections.Generic;
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
        public static Dispose.One Scoped(this Action thiz) => new(thiz);

        /// Returns an action which calls Thiz only the first time it's invoked, none of the following.
        public static Action FirstCalling(this Action thiz)
        => () => { thiz?.Invoke(); thiz = null; };

        /// Returns a factory; thiz is called when the last action returned from the factory is invoked.
        public static Func<T, Action> LastCalling<T>(this Action thiz)
        {
            Dictionary<T, int> count = new();
            Action Checkout(T value)
            {
                count[value] = (count.TryGetValue(value, out var i) ? i : 0) + 1;
                void Redeem()
                {
                    count.TryGetValue(value, out var o).OrThrow();
                    (--o < 0).AndThrow();
                    if (o > 0) { count[value] = 0; return; }
                    count.Remove(value);
                    if (count.Count <= 0)
                    {
                        thiz.OrThrow().Invoke();
                        thiz = null;
                    }
                }
                return Redeem;
            }
            return Checkout;
        }

        public static Action<float> GetLerpAction<T>(this IArith<T> thiz, T start, T end, Action<T> action)
        => (f) => action(thiz.Lerp(start, end, f));

        /// Calls underlying `thiz` with `transform(input)` each time.
        public static Action<TNew> Curried<TOld, TNew>(this Action<TOld> thiz, Func<TNew, TOld> transform)
        => (tnew) => thiz(transform(tnew));
        /// Calls underlying `thiz` with `transform()` each time.
        public static Action Curried<T>(this Action<T> thiz, Func<T> transform)
        => () => thiz(transform());
        /// Calls underlying `thiz` with `value` each time.
        public static Action Curried<T>(this Action<T> thiz, T value)
        => () => thiz(value);
        /// Calls underlying `thiz` with `true` each time.
        public static Action Curried(this Action<bool> thiz) => thiz.Curried(true);

        /// Calls underlying `thiz` with `transform(input)` each time.
        public static Func<TNew, TOut> Curried<TOld, TNew, TOut>(this Func<TOld, TOut> thiz, Func<TNew, TOld> transform)
        => (tnew) => thiz(transform(tnew));
        /// Calls underlying `thiz` with `transform()` each time.
        public static Func<TOut> Curried<TIn, TOut>(this Func<TIn, TOut> thiz, Func<TIn> transform)
        => () => thiz(transform());
        /// Calls underlying `thiz` with `value, transform()` each time.
        public static Func<TNew, TOut> Curried<TConst, TOld, TNew, TOut>(
            this Func<TConst, TOld, TOut> thiz,
            TConst value,
            Func<TNew, TOld> transform
        ) => (tnew) => thiz(value, transform(tnew));
        /// Calls underlying `thiz` with `value, transform()` each time.
        public static Func<TIn, TOut> Curried<TConst, TIn, TOut>(
            this Func<TConst, TIn, TOut> thiz,
            TConst value
        ) => (t) => thiz(value, t);

        /// Transform outputs on return (on each call).
        public static Func<TNew> Piped<TOld, TNew>(this Func<TOld> thiz, Func<TOld, TNew> transform)
        => () => transform(thiz());
        /// Transform outputs on return (on each call).
        public static Func<TNew> Piped<TNew>(this Func<bool> thiz, TNew onTrue, TNew onFalse = default)
        => () => thiz() ? onTrue : onFalse;

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
        public static readonly Func<bool, bool> BoolIdentity = b => b;
        public static Action MakeSetter(out Func<bool> getter) => BoolIdentity.MakeSetter(out getter).Curried();
        public static Func<bool> MakeGetter(out Action setter)
        {
            setter = MakeSetter(out Func<bool> getter);
            return getter;
        }

        public static void Invoke(this IEnumerable<Action> thiz)
        { if (thiz != null) foreach (Action a in thiz) a.Invoke(); }
        public static void Invoke<T1>(this IEnumerable<Action<T1>> thiz, T1 t1)
        { if (thiz != null) foreach (Action<T1> a in thiz) a.Invoke(t1); }
        public static void Invoke<T1, T2>(this IEnumerable<Action<T1, T2>> thiz, T1 t1, T2 t2)
        { if (thiz != null) foreach (Action<T1, T2> a in thiz) a.Invoke(t1, t2); }
    }
}