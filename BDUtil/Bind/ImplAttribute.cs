using System;
using System.Collections.Generic;

namespace BDUtil.Bind
{
    /// Binds this type as the (preferred, default) implementation of some more generic type.
    /// This is placed on the implementation pointing at the generic (see Interface, which can also be an abstract class etc).
    /// As a consequence, any assembly analysis has to happen fairly late.
    public sealed class ImplAttribute : BindAttribute
    {
        /// Null means "for every type I match" (rather than specifying this multiple times).
        /// This is great esp for `struct Foo : IBar {}` patterns, where there only is one.
        /// To save time, you never match anything in UnityEngine or System (to prevent your random struct from being the best ValueType, etc).
        public Type Interface = null;
        public ImplAttribute(Type @interface = default) => Interface = @interface;

        public override IEnumerable<object> GetKeys(Type type)
        {
            if (Interface != null)
            {
                yield return Interface;
                yield break;
            }
            for (Type parent = type; AllowSuper(parent); parent = parent.BaseType)
                yield return parent;
            foreach (Type parent in type.GetInterfaces())
                if (AllowSuper(parent)) yield return parent;
        }
        bool AllowSuper(Type type)
        {
            // Don't match into any subclass interfaces in System or Unity.
            // this maybe isn't needed
            switch (type?.Namespace)
            {
                case null: return false;
                case var s when s.StartsWith("System."):  // Fallthrough
                case var u when u.StartsWith("Unity"):  // Fallthrough
                    return false;
            }
            return true;
        }
    }
}