using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    public static class SerializedProperties
    {
        private const BindingFlags FieldBindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        private const BindingFlags PropertyBindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
        static readonly Type ListType = typeof(List<>);
        static readonly Type SubtypeType = typeof(Subtype<>);
        /// Unwraps List<T>, T[], Subtype<T>, and ofc T => T.
        public static Type GetUnderlyingType(this Type thiz)
        {
            Type type = thiz;
            if (type.IsArray) type = type.GetElementType();
            bool @continue = true;
            while (@continue && type.IsConstructedGenericType) switch (type.GetGenericTypeDefinition())
                {
                    case var x when x == ListType: type = type.GetGenericArguments()[0]; break;
                    case var x when x == SubtypeType: type = type.GetGenericArguments()[0]; break;
                    default: @continue = false; break;
                }
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
            var (AssemblyName, ClassName) = GetSplitNamesFromTypename(stringType);
            var realType = Type.GetType($"{ClassName}, {AssemblyName}");
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

        public static object GetTargetObjectWithProperty(this SerializedProperty thiz)
        {
            string path = thiz.propertyPath.Replace(".Array.data[", "[");
            object obj = thiz.serializedObject.targetObject;
            string[] elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                string element = elements[i];
                int lbrace = element.IndexOf("[");
                if (lbrace > 0)
                {
                    string elementName = element[..lbrace];
                    int rbrace = element.IndexOf("]");
                    int index = Convert.ToInt32(element[(lbrace + 1)..rbrace]);
                    obj = GetListValue(obj, elementName, index);
                }
                else obj = GetValue(obj, element);
            }

            return obj;
        }
        private static object GetValue(object source, string name)
        {
            if (source == null) return null;
            for (Type type = source.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, FieldBindingAttr);
                if (field != null) return field.GetValue(source);
                PropertyInfo property = type.GetProperty(name, PropertyBindingAttr);
                if (property != null) return property.GetValue(source, null);
            }
            return null;
        }

        private static object GetListValue(object source, string name, int index)
        {
            if (GetValue(source, name) is not IEnumerable enumerable) return null;
            if (enumerable is IList list) return index < list.Count ? list[index] : null;
            IEnumerator enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++) if (!enumerator.MoveNext()) return null;
            return enumerator.Current;
        }
    }
}