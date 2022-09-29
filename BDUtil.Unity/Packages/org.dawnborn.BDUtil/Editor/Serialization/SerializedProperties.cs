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

        public static Type GetTargetValueType(this SerializedProperty thiz)
        {
            var ptr = thiz.GetTargetParent(out string suffix, out int _);
            return GetTargetChildMember(ptr, suffix) switch
            {
                null => null,
                PropertyInfo property => property.PropertyType.GetUnderlyingType(),
                FieldInfo field => field.FieldType.GetUnderlyingType(),
                _ => throw new NotSupportedException(),
            };
        }
        public static object GetTargetValue(this SerializedProperty thiz)
        {
            var ptr = thiz.GetTargetParent(out string suffix, out int suffIndex);
            if (suffIndex < 0) return GetTargetChildData(ptr, suffix);
            return GetTargetChildArray(ptr, suffix, suffIndex);
        }
        public static bool SetTargetValue(this SerializedProperty thiz, object value)
        {
            var ptr = thiz.GetTargetParent(out string suffix, out int suffIndex);
            if (suffIndex < 0) return SetTargetChildData(ptr, suffix, value);
            return SetTargetChildArray(ptr, suffix, suffIndex, value);
        }
        /// Get the value of this serialized property in real-object space.
        /// Rewrites the property so that `x.Array.data[0].y.Array.data[1]` is `x[0].y[1]`; returns `x[0]` and the string "y", int 1.
        /// If it had been `...data[x]....y` (no trailing index), i would be negative.
        public static object GetTargetParent(this SerializedProperty thiz, out string suffix, out int suffixIndex)
        {
            string path = thiz.propertyPath.Replace(".Array.data[", "[");
            object obj = thiz.serializedObject.targetObject;
            string[] elements = path.Split('.');
            for (int i = 0; i < elements.Length - 1; i++)
            {
                (suffix, suffixIndex) = ParsePathComponent(elements[i]);
                if (suffixIndex < 0) obj = GetTargetChildData(obj, suffix);
                else obj = GetTargetChildArray(obj, suffix, suffixIndex);
            }
            (suffix, suffixIndex) = ParsePathComponent(elements[^1]);
            return obj;
        }
        private static (string key, int index) ParsePathComponent(string element)
        {
            int lbrace = element.IndexOf("[");
            if (lbrace > 0)
            {
                string key = element[..lbrace];
                int rbrace = element.IndexOf("]");
                int index = Convert.ToInt32(element[(lbrace + 1)..rbrace]);
                return (key, index);
            }
            return (element, -1);
        }
        private static MemberInfo GetTargetChildMember(object source, string name)
        {
            if (source == null) return null;
            for (Type type = source.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, FieldBindingAttr);
                if (field != null) return field;
                PropertyInfo property = type.GetProperty(name, PropertyBindingAttr);
                if (property != null) return property;
            }
            return null;
        }
        private static object GetTargetChildData(object source, string name)
        => GetTargetChildMember(source, name) switch
        {
            null => null,
            FieldInfo field => field.GetValue(source),
            PropertyInfo property => property.GetValue(source),
            _ => throw new NotSupportedException(),
        };
        private static object GetTargetChildArray(object source, string name, int index)
        {
            if (GetTargetChildData(source, name) is not IEnumerable enumerable) return null;
            if (enumerable is IList list) return index < list.Count ? list[index] : null;
            IEnumerator enumerator = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++) if (!enumerator.MoveNext()) return null;
            return enumerator.Current;
        }

        private static bool SetTargetChildData(object source, string name, object toValue)
        {
            switch (GetTargetChildMember(source, name))
            {
                case null: return false;
                case FieldInfo field: field.SetValue(source, toValue); return true;
                case PropertyInfo property: property.SetValue(source, toValue); return true;
                default: throw new NotSupportedException();
            }
        }
        private static bool SetTargetChildArray(object source, string name, int index, object toValue)
        {
            switch (GetTargetChildData(source, name))
            {
                case null: return false;
                case object[] a: a[index] = toValue; return true;
                case IList l: l[index] = toValue; return true;
                default: return false;
            }
        }
    }
}