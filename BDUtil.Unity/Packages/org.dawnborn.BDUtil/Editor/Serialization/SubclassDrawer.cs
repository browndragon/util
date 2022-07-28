using System;
using System.Collections.Generic;
using System.Reflection;
using BDUtil;
using BDUtil.Bind;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    [CustomPropertyDrawer(typeof(SubclassAttribute))]
    public class SubclassDrawer : PropertyDrawer
    {
        // Typecache of base->[concrete]subclasses \ UnityEngine.Object.
        // TODO: clear?
        static readonly Dictionary<Type, SubclassData> cache = new();
        // per-instance (?? Reuse safe? Seems to be...) subclass drawer.
        // TODO: clear? Less critical, destroying instance kills it.
        // readonly Dictionary<string, int> index = new();
        SubclassData GetCachedSubclassData(Type @base)
        {
            if (cache.TryGetValue(@base, out SubclassData cached)) return cached;
            cached = new SubclassData(@base);
            cached.Calculate();
            cache.Add(@base, cached);
            return cached;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Necessary per docs.
            EditorGUI.BeginProperty(position, label, property);
            try
            {
                // Don't support multi!
                // If you try, the multiedit sets fields wrong after doing some amount of editing.
                // (I guess it uses wrong offset from wrong element?)
                if (property.serializedObject.isEditingMultipleObjects)
                {
                    EditorGUI.HelpBox(position, $"Can't multiedit subclass refs", MessageType.Warning);
                    return;
                }
                if (property.propertyType != SerializedPropertyType.ManagedReference) return;

                Type baseType = fieldInfo.FieldType;
                // TODO: support per-field preferences, renames, etc?
                SubclassData subs = GetCachedSubclassData(/*attribute as SubclassAttribute,*/ baseType);
                if (subs.Subs == null || subs.Subs.Count <= 1)  // 1 for null.
                {
                    EditorGUI.HelpBox(position, $"Can't find subclasses for {baseType}", MessageType.Warning);
                    return;
                }
                property.serializedObject.Update();
                Type hasType = property.managedReferenceValue?.GetType();
                int prevIndex = 0;
                // if (!index.TryGetValue(property.propertyPath, out int prevIndex)) {
                prevIndex = subs.Find(hasType);
                // }
                /// Really, > Count should NEVER happen. But?
                if (prevIndex < 0 || prevIndex >= subs.Subs.Count)
                {
                    Debug.LogWarning($"Can't find {hasType} anymore!");
                    prevIndex = 0;
                }
                int currentIndex = EditorGUI.Popup(GetPopupPosition(position), prevIndex, subs.SubNames);
                if (currentIndex < 0 || currentIndex >= subs.Subs.Count)
                {
                    Debug.LogWarning($"User selected {currentIndex} outside [0, {subs.Subs.Count}); setnull");
                    currentIndex = 0;
                }
                // index[property.propertyPath] = currentIndex;

                Type selectedType = subs.Subs[currentIndex];
                if (hasType != selectedType)
                {
                    property.managedReferenceValue =
                        ((selectedType == null) ? null : Activator.CreateInstance(selectedType));
                    property.serializedObject.ApplyModifiedProperties();
                    Debug.Log($"Changing types {prevIndex}=>{currentIndex} on {property.serializedObject?.targetObject}/{property.propertyPath} {hasType} => {selectedType}");
                }
                // Then recurse & draw whatever type it has, too...
                EditorGUI.PropertyField(position, property, label, true);
            }
            finally { EditorGUI.EndProperty(); }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return EditorGUIUtility.singleLineHeight;
            return EditorGUI.GetPropertyHeight(property, true);
        }
        Rect GetPopupPosition(Rect currentPosition)
        {
            Rect popupPosition = new Rect(currentPosition);
            popupPosition.width -= EditorGUIUtility.labelWidth;
            popupPosition.x += EditorGUIUtility.labelWidth;
            popupPosition.height = EditorGUIUtility.singleLineHeight;
            return popupPosition;
        }
    }
}