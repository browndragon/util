using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    /// Replace containers with their proxy. This assumes they're serializable at all!
    [
        CustomPropertyDrawer(typeof(BiMap<,>)),
        CustomPropertyDrawer(typeof(FKeySet<,>)),
        // CustomPropertyDrawer(typeof(KeySet<,>)),
        CustomPropertyDrawer(typeof(Map<,>)),
        CustomPropertyDrawer(typeof(MultiMap<,>)),
        CustomPropertyDrawer(typeof(Set<>)),
        CustomPropertyDrawer(typeof(Table<,,>)),
    ]
    public class ContainerDrawer : PropertyDrawer
    {
        const string Path = "Proxy";
        public override void OnGUI(Rect rectangle, SerializedProperty property, GUIContent label)
        {
            var property2 = property.FindPropertyRelative(Path);
            EditorGUI.PropertyField(rectangle, property2, label, property2.isExpanded);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var property2 = property.FindPropertyRelative(Path);
            return EditorGUI.GetPropertyHeight(property2, property2.isExpanded);
        }
    }
}