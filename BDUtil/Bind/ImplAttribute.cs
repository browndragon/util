using System;

namespace BDUtil.Bind
{
    /// Binds this type as the (preferred, default) implementation of some more generic type.
    /// Every type theoretically binds itself (see TypedAttribute.Self), though that's not usually necessary.
    /// This is placed on the implementation pointing at the generic (see Interface, which can also be an abstract class etc);
    /// for concrete subclasses of unbound generic interfaces, there may not *be* a single location to point parent->child!
    /// As a consequence, any assembly analysis has to happen fairly late.
    public sealed class ImplAttribute : BindAttribute
    {
        /// Null means "for every type I match" (rather than specifying this multiple times).
        public Type Interface;
        /// Preference order when there are multiple ImplAttributes present for the same object.
        /// Positive ranks occur before other options ("favored"); negative ranks occur after other options ("disfavored").
        public int Rank = 0;
        public ImplAttribute() => Interface = null;
        public ImplAttribute(Type @interface) => Interface = @interface;
        /// For complex types, like those using compound type restrictions.
        public static ImplAttribute Of<T>(int rank = 0) => new(typeof(T)) { Rank = rank };
        public override object GetKey(Type type) => Interface ?? type;
        public override string ToString() => Interface?.ToString();
    }
}