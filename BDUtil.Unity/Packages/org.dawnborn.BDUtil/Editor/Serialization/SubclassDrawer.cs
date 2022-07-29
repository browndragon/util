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
        public SubclassAttribute SubclassAttribute => attribute as SubclassAttribute;
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

                Type baseType = GetFieldType(property);
                // TODO: support per-field preferences, renames, etc?
                SubclassData subs = GetCachedSubclassData(/*attribute as SubclassAttribute,*/ baseType);
                if (subs.Subs == null || subs.Subs.Count <= 1)  // 1 for null.
                {
                    EditorGUI.HelpBox(position, $"Can't find subclasses for {baseType}", MessageType.Warning);
                    return;
                }
                Type hasType = property.managedReferenceValue?.GetType();
                int prevIndex = subs.Find(hasType);
                if (prevIndex < 0 || prevIndex >= subs.Subs.Count)
                {
                    Debug.LogWarning($"Can't find prev {hasType} anymore!");
                    int preferredIndex = subs.Find(SubclassAttribute.Default);
                    if (preferredIndex < 0 || preferredIndex >= subs.Subs.Count)
                    {
                        Debug.LogWarning($"Can't find pref {SubclassAttribute.Default} anymore!");
                        preferredIndex = 0;
                    }
                    prevIndex = preferredIndex;
                }
                int currentIndex = EditorGUI.Popup(GetPopupPosition(position), prevIndex, subs.SubNames);
                if (currentIndex < 0 || currentIndex >= subs.Subs.Count)
                {
                    Debug.LogWarning($"User selected {currentIndex} outside [0, {subs.Subs.Count}); suppressing");
                    currentIndex = prevIndex;
                }

                Type selectedType = subs.Subs[currentIndex];
                if (hasType != selectedType)
                {
                    property.serializedObject.Update();
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
            Rect popupPosition = new(currentPosition);
            popupPosition.width -= EditorGUIUtility.labelWidth;
            popupPosition.x += EditorGUIUtility.labelWidth;
            popupPosition.height = EditorGUIUtility.singleLineHeight;
            return popupPosition;
        }
        Type GetFieldType(SerializedProperty property)
        {
            var realPropertyType = GetRealTypeFromTypename(property.managedReferenceFieldTypename);
            if (realPropertyType == null)
                Debug.LogError($"Can not get field type of managed reference: {property.managedReferenceFieldTypename}");
            return realPropertyType;
        }

        // Gets real type of managed reference's field typeName
        Type GetRealTypeFromTypename(string stringType)
        {
            var names = GetSplitNamesFromTypename(stringType);
            var realType = Type.GetType($"{names.ClassName}, {names.AssemblyName}");
            return realType;
        }
        (string AssemblyName, string ClassName) GetSplitNamesFromTypename(string typename)
        {
            if (string.IsNullOrEmpty(typename))
                return ("", "");

            var typeSplitString = typename.Split(char.Parse(" "));
            var typeClassName = typeSplitString[1];
            var typeAssemblyName = typeSplitString[0];
            return (typeAssemblyName, typeClassName);
        }
    }
}