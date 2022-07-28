

using System;
using UnityEngine;

namespace BDUtil
{
    /// Marks an *also `SerializeRef`* field as a subclass-selectable one.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SubclassAttribute : PropertyAttribute
    {
        public SubclassAttribute() { }
    }
}