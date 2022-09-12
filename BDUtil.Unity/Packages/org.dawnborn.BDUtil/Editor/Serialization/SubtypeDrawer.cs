using System;
using System.Collections.Generic;
using System.Reflection;
using BDUtil.Serialization.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    /// Selects things that are Subtype<T>, serializable type tokens -- OR tagged with SubtypeAttribute -- OR both.
    [CustomPropertyDrawer(typeof(Subtype<>))]
    [CustomPropertyDrawer(typeof(SubtypeAttribute))]
    public class SubtypeDrawer : AbstractTypeDrawer
    {
        static Type GetTypeByString(string s) => (s == null || s.Length <= 0) ? null : Type.GetType(s);
        static string GetSerializedType(SerializedProperty @base)
        => @base.FindPropertyRelative("serialized").stringValue;
        static void SetSerializedType(SerializedProperty @base, string value)
        => @base.FindPropertyRelative("serialized").stringValue = value;
        protected override Choices GetChoices(SerializedProperty property)
        => property.propertyType == SerializedPropertyType.ManagedReference
        ? GetChoicesMR(property) : GetChoicesCN(property);
        Choices GetChoicesMR(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference) return default;
            Type baseType = property.GetManagedReferenceFieldType();
            Type hasType = property.managedReferenceValue?.GetType();
            SubtypeAttribute attribute = fieldInfo.GetCustomAttribute<SubtypeAttribute>();

            // TODO: support per-field preferences, renames, etc?
            Choices choices = GetCachedSubclassData(new(
                baseType,
                attribute
            ), attribute?.PrintDebug ?? false);
            List<Type> objects = (List<Type>)choices.Objects.OrThrow();
            // TODO: cache me?
            choices.Index = objects.IndexOf(hasType);
            if (choices.Index < 0 || choices.Index >= objects.Count)
            {
                Debug.LogWarning($"Can't find prev {hasType} anymore!");
                int preferredIndex = objects.IndexOf(attribute?.Default);
                if (preferredIndex < 0 || preferredIndex >= choices.Objects.Count)
                {
                    Debug.LogWarning($"Can't find pref {attribute?.Default} anymore!");
                    preferredIndex = 0;
                }
                choices.Index = preferredIndex;
            }
            return choices;
        }
        Choices GetChoicesCN(SerializedProperty property)
        {
            Type baseType = fieldInfo.FieldType.GetUnderlyingType();
            // TODO: support per-field preferences, renames, etc?
            SubtypeAttribute attribute = fieldInfo.GetCustomAttribute<SubtypeAttribute>();
            Choices choices = GetCachedSubclassData(
                new(baseType, attribute),
                attribute?.PrintDebug ?? false
            );

            string hasTypeString = GetSerializedType(property);
            Type hasType = GetTypeByString(hasTypeString);
            // TODO: cache me?
            choices.Index = ((IList<Type>)choices.Objects).IndexOf(hasType);
            if (choices.Index < 0 || choices.Index >= choices.Objects.Count)
            {
                Debug.LogWarning($"Can't find prev {hasTypeString} under {baseType} anymore!");
                choices.Index = 0;
            }
            return choices;
        }

        protected override void Update(SerializedProperty property, Type selectedType)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                UpdateMR(property, selectedType);
                return;
            }
            UpdateCN(property, selectedType);
        }
        void UpdateCN(SerializedProperty property, Type selectedType)
        {
            string hasTypeString = GetSerializedType(property);
            string wantsTypeString = selectedType?.AssemblyQualifiedName;
            if (hasTypeString != wantsTypeString)
            {
                property.serializedObject.Update();
                SetSerializedType(property, wantsTypeString);
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        void UpdateMR(SerializedProperty property, Type selectedType)
        {
            Type hasType = property.managedReferenceValue.OrThrow().GetType();
            if (hasType != selectedType)
            {
                property.serializedObject.Update();
                property.managedReferenceValue =
                    ((selectedType == null) ? null : Activator.CreateInstance(selectedType));
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        protected override float InnerHeight(SerializedProperty property)
        => property.propertyType == SerializedPropertyType.ManagedReference
            ? EditorGUI.GetPropertyHeight(property, true)
            : base.InnerHeight(property);

        protected override void DrawInnerField(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                // Then recurse & draw whatever type it has, too...
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }
            base.DrawInnerField(position, property, label);
        }
    }
}