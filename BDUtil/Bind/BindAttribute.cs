using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Bind
{
    /// Indicates a type "provides" or is "bound" to a key in a discoverable, registrable way.
    /// For instance, `List<>` is _the_ implementation of `IList<>`, so it would be nice to be able
    /// to satisfy that arbitrarily.
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum
        , AllowMultiple = true)]
    public abstract class BindAttribute : Attribute
    {
        public int Rank = 0;
        /// This would be some `TK` instead of `object` but generic attributes are a future feature.
        /// Still, it's on you: be consistent about your registry-to-keytypes; since this is abstract,
        /// you can appropriately subclass.
        public abstract IEnumerable<object> GetKeys(Type type);
    }
}