

using System;
using UnityEngine;

namespace BDUtil.Serialization
{
    // Can be applied to any field, causing it to invoke the method OnChange (or: button!).
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public sealed class InvokeAttribute : PropertyAttribute
    {
        public string MethodName;
        public Invoke.Suppress Suppresses;
        public InvokeAttribute(string methodName) => MethodName = methodName;
    }

    public static class Invoke
    {
        /// Special styling rules: When the invokeattributedrawer is called on one of _these_, it
        /// styles it as a button.
        /// This also gives you a location to stick the label name.
        [Serializable]
        public struct Button { }
        // Do/don't show the button or send the message in some circumstances...
        public enum Suppress
        {
            Never = default,
            Editor = 1 << 0,
            Play = 1 << 1,
        }
    }
}