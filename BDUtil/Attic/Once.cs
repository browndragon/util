using System;
using System.Collections.Generic;

namespace BDUtil.Attic
{
    /// Useful for having static code which should only run once per type.
    /// The TContext param should be an `internal` type controlling in what context you mean "just once"
    /// based on T (for instance, you might need to register a single assembly in multiple contexts).
    public static class Once<TContext, T>
    {
        internal static bool value = false;
        /// Returns true if this is actually the first time Once<T1, T2>.Init() was hit.
        public static bool Init()
        {
            if (value) return false;
            value = true;
            ResetOnce<TContext>.resetAction += ResetOnce<TContext, T>.Reset;
            return true;
        }
    }
    public static class Once<TContext>
    {
        public static bool Init() => Once<TContext, TContext>.Init();
    }

    /// Testing etc; clears Once<TContext, T> for all T.
    public class ResetOnce<TContext>
    {
        static internal event Action resetAction = default;
        public static void ResetAll()
        {
            resetAction?.Invoke();
            resetAction = default;
        }
    }
    /// Testing etc; clears Once<TContext, T> for specific T.
    public class ResetOnce<TContext, T>
    {
        public static void Reset() => Once<TContext, T>.value = false;
    }
}