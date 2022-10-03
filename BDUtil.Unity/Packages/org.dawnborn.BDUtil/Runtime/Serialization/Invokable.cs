

using System;

namespace BDUtil.Serialization
{
    // Can be applied to any *METHOD*; causes it to
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class InvokableAttribute : Attribute
    {
        /// Lets you specify a relative importance within the layoutgroup.
        public int order;
        /// Lets you specify a specific Invoke.Layout point at which to lay it out.
        public string LayoutName;
        /// Lets you override the button's name (otherwise, as method name).
        public string Label;
        public Invokable.Suppress Suppresses;
    }

    public static class Invokable
    {
        /// Rally point for Invokes.
        [Serializable]
        public struct Layout { }
        // Do/don't show the button or send the message in some circumstances...
        public enum Suppress
        {
            Never = default,
            Editor = 1 << 0,
            Play = 1 << 1,
        }
    }
}