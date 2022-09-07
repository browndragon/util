using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(Ref<>))]
    public class RefDrawer : PropertyDrawer
    {
        readonly Dictionary<string, UnityEngine.Object> references = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("assetPath"));
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty outer = property;
            property = outer.FindPropertyRelative("assetPath");

            Type objectType = fieldInfo.FieldType.GetUnderlyingType();
            string assetPath = property.stringValue;

            if (!references.TryGetValue(property.propertyPath, out UnityEngine.Object target))
            {
                if (assetPath.IsEmpty()) target = references[property.propertyPath] = null;
                else target = references[property.propertyPath] = AssetDatabase.LoadAssetAtPath(assetPath, objectType);
            }
            /// Now: we either have a value, or pointed at nothing.
            EditorGUI.BeginChangeCheck();
            // Draw our object field.
            target = EditorGUI.ObjectField(position, label, target, objectType, false);
            if (EditorGUI.EndChangeCheck()) OnSelectionMade(target, property);
        }

        protected virtual void OnSelectionMade(UnityEngine.Object newSelection, SerializedProperty property)
        {
            string assetPath = newSelection == null
                ? string.Empty
                : AssetDatabase.GetAssetPath(newSelection);
            references[property.propertyPath] = newSelection;
            property.stringValue = assetPath;
        }
    }
}