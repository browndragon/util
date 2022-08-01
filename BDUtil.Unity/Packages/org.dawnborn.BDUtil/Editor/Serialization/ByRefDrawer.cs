using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    /// Selects things tagged SubclassAttribute.
    /// More generally, finds-and-matches serialized subtypes.
    [CustomPropertyDrawer(typeof(SubtypeAttribute))]
    public class ByRefDrawer : AbstractTypeDrawer
    {
        internal SubtypeAttribute SubtypeAttribute => attribute as SubtypeAttribute;
        internal override Choices GetChoices(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference) return default;
            Type baseType = property.GetManagedReferenceFieldType();
            Type hasType = property.managedReferenceValue?.GetType();

            // TODO: support per-field preferences, renames, etc?
            Choices choices = GetCachedSubclassData(/*attribute as SubclassAttribute,*/ baseType);
            List<Type> objects = (List<Type>)choices.Objects.OrThrow();
            // TODO: cache me?
            choices.Index = objects.IndexOf(hasType);
            if (choices.Index < 0 || choices.Index >= objects.Count)
            {
                Debug.LogWarning($"Can't find prev {hasType} anymore!");
                int preferredIndex = objects.IndexOf(SubtypeAttribute.Default);
                if (preferredIndex < 0 || preferredIndex >= choices.Objects.Count)
                {
                    Debug.LogWarning($"Can't find pref {SubtypeAttribute.Default} anymore!");
                    preferredIndex = 0;
                }
                choices.Index = preferredIndex;
            }
            return choices;
        }
        internal override void Update(SerializedProperty property, Type selectedType)
        {
            Type hasType = property.managedReferenceValue?.GetType();
            if (hasType != selectedType)
            {
                property.serializedObject.Update();
                property.managedReferenceValue =
                    ((selectedType == null) ? null : Activator.CreateInstance(selectedType));
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        internal override float InnerHeight(SerializedProperty property)
        => EditorGUI.GetPropertyHeight(property, true);

        internal override void DrawInnerField(Rect position, SerializedProperty property, GUIContent label)
        {
            // Then recurse & draw whatever type it has, too...
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}