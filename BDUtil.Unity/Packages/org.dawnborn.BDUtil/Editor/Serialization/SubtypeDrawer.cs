using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    /// Selects things that are Subtype<T>, serializable type tokens.
    [CustomPropertyDrawer(typeof(Subtype<>))]
    public class SubtypeDrawer : AbstractTypeDrawer
    {
        static Type GetTypeByString(string s) => (s == null || s.Length <= 0) ? null : Type.GetType(s);
        static string GetSerializedType(SerializedProperty @base)
        => @base.FindPropertyRelative("serialized").stringValue;
        static void SetSerializedType(SerializedProperty @base, string value)
        => @base.FindPropertyRelative("serialized").stringValue = value;
        internal override Choices GetChoices(SerializedProperty property)
        {
            Type baseType = fieldInfo.FieldType.GetUnderlyingType();
            string hasTypeString = GetSerializedType(property);
            Type hasType = GetTypeByString(hasTypeString);

            // TODO: support per-field preferences, renames, etc?
            Choices choices = GetCachedSubclassData(/*attribute as SubclassAttribute,*/ baseType);

            // TODO: cache me?
            choices.Index = ((IList<Type>)choices.Objects).IndexOf(hasType);
            if (choices.Index < 0 || choices.Index >= choices.Objects.Count)
            {
                Debug.LogWarning($"Can't find prev {hasTypeString} under {baseType} anymore!");
                choices.Index = 0;
            }
            return choices;
        }
        internal override void Update(SerializedProperty property, Type selectedType)
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
    }
}