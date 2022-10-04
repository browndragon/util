using System;

namespace BDUtil
{
    /// A type with no representable elements.
    public sealed class Void
    {
        private Void() => throw new NotImplementedException();
    }
}
