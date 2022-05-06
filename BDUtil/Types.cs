using System;
using System.Collections.Generic;

namespace BDUtil
{
    /// Utilities to walk a type's hierarchy.
    public static class Types
    {
        public static bool NoBlacklist(Type _) => false;
        public static bool Blacklist(Type token) => token != null && token.Namespace switch
        {
            nameof(System) => true,
            var systemDot when token.Namespace.StartsWith(nameof(System)) => true,
            _ => false,
        };

        public static IEnumerable<Type> GetTypesInstance(object instance, Func<Type, bool> blacklist = default) => GetTypesToken(instance.GetType(), blacklist);
        public static IEnumerable<Type> GetTypesToken(Type token, Func<Type, bool> blacklist = default)
        {
            if (blacklist == default) blacklist = Blacklist;
            if (blacklist(token)) yield break;

            // Walk interfaces.
            // Sadly, we can't cut out at a known stupid interface; this just unfolds everything we implement.
            foreach (Type @interface in token.GetInterfaces())
            {
                if (blacklist(@interface)) continue;
                yield return @interface;
            }
            // Walk inheritance hierarchy.
            for (Type next = token; next != null; next = next.BaseType)
            {
                // Once we blacklist on the chain, we're done.
                if (blacklist(next)) break;
                yield return next;
            }
        }
    }
}
