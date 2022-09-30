using System;
using System.IO;
using System.Reflection;
using BDUtil.Serialization;
using UnityEditor;
using UnityEngine;

namespace BDUtil
{
    public static class AnyScriptableObject
    {
        [MenuItem("Assets/Create/BDUtil/Script Asset", false, -1)]
        public static void CreateScriptableObject()
        {
            EditorUtils.CreateScriptableObjectOfType(((MonoScript)Selection.activeObject).GetClass(), true);
        }

        [MenuItem("Assets/Create/BDUtil/Script Asset", true)]
        public static bool CreateScriptableObjectValidate()
        {
            return Selection.activeObject is MonoScript monoScript && IsScriptableObject(monoScript.GetClass());
        }

        private static bool IsScriptableObject(System.Type type) => typeof(ScriptableObject).IsAssignableFrom(type);
    }
}