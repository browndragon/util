

using System;
using UnityEngine;

namespace BDUtil
{
    /// Marks a field as requiring a reflective method call when changed.
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OnChangeAttribute : PropertyAttribute
    {
        public string MethodName;
        // Do/don't send the message in some circumstances...
        public enum Suppress
        {
            Never = default,
            Editor = 1 << 0,
            Play = 1 << 1,
        }
        public Suppress Suppresses;

        /// If set, this replaces the decorated field and simply uses it as a button.
        public bool AsButton = false;
        public OnChangeAttribute(string methodName) => MethodName = methodName;
    }
}