using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    public static class SerializedProperties
    {
        static readonly Type ListType = typeof(List<>);
        static readonly Type SubtypeType = typeof(Subtype<>);

        /// Unwraps List<T>, T[], Subtype<T>, and ofc T => T.
        public static Type GetUnderlyingType(this Type thiz)
        {
            Type type = thiz;
            if (type.IsArray) type = type.GetElementType();
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == ListType) type = type.GetGenericArguments()[0];
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == SubtypeType) type = type.GetGenericArguments()[0];
            return type;
        }

        /// Gets real type of managed reference
        public static Type GetManagedReferenceFieldType(this SerializedProperty property)
        {
            var realPropertyType = GetRealTypeFromTypename(property.managedReferenceFieldTypename);
            if (realPropertyType != null)
                return realPropertyType;

            Debug.LogError($"Can not get field type of managed reference : {property.managedReferenceFieldTypename}");
            return null;
        }

        /// Gets real type of managed reference's field typeName
        public static Type GetRealTypeFromTypename(string stringType)
        {
            var names = GetSplitNamesFromTypename(stringType);
            var realType = Type.GetType($"{names.ClassName}, {names.AssemblyName}");
            return realType;
        }

        /// Get assembly and class names from typeName
        public static (string AssemblyName, string ClassName) GetSplitNamesFromTypename(string typename)
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