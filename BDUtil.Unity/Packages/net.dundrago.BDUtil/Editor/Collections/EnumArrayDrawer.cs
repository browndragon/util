using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BDUtil.Editor
{
    [CustomPropertyDrawer(typeof(EnumArray<,>))]
    public class EnumArrayDrawer : PropertyDrawer
    {
        const string Path = "Data";
        string[] _names;
        string[] Names => _names ??= Enum.GetNames(fieldInfo.FieldType.GetGenericArguments()[0]);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            label = EditorGUI.BeginProperty(position, label, property);
            Rect single = position;
            single.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(single, property.isExpanded, label, true);
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            var data = property.FindPropertyRelative(Path);
            if (property.isExpanded && data != null && data.isArray)
            {
                for (int i = 0; i < data.arraySize; ++i)
                {
                    single.y += single.height + EditorGUIUtility.standardVerticalSpacing;
                    var child = data.GetArrayElementAtIndex(i);
                    label = new GUIContent(Names[i]);
                    single.height = EditorGUI.GetPropertyHeight(child, label);
                    EditorGUI.PropertyField(single, child, label, child.isExpanded);
                }
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float total = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return total;
            var data = property.FindPropertyRelative(Path);
            if (data == null || !data.isArray) return total;
            total += data.arraySize * EditorGUIUtility.standardVerticalSpacing;
            for (int i = 0; i < data.arraySize; ++i) total += EditorGUI.GetPropertyHeight(data.GetArrayElementAtIndex(i));
            return total;
        }
    }
}