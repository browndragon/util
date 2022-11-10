using System;
using UnityEngine;

namespace BDUtil.Serialization
{
    // Per https://forum.unity.com/threads/editor-tool-better-scriptableobject-inspector-editing.484393/ .
    public sealed class ExpandableAttribute : PropertyAttribute
    {
        public Type RestrictTo;
    }
}