using System;
using System.Reflection;
using BDUtil.Serialization;
using UnityEditor;
using UnityEngine;

namespace BDUtil
{
    public static class AnyScriptableObject
    {
        [MenuItem("Assets/Create/BDUtil/Script Asset", false, 0)]
        public static void CreateScriptableObject()
        {
            MonoScript selection = (MonoScript)Selection.activeObject;
            Type type = selection.GetClass();
            StaticAsset.FilePathAttribute fpA = type.GetCustomAttribute<StaticAsset.FilePathAttribute>();
            // First try the "proper location" (if it has one).
            string path = fpA?.GetFilePath(type);
            if (path == null)
            {
                // Otherwise, the current location.
                string assetPath = AssetDatabase.GetAssetPath(selection.GetInstanceID());
                path = $"{System.IO.Directory.GetParent(assetPath)}/{type.Name}.asset";
            }
            UnityEngine.Object asset = ScriptableObject.CreateInstance(type);
            ProjectWindowUtil.CreateAsset(asset, path);
        }

        [MenuItem("Assets/Create/BDUtil/Script Asset", true)]
        public static bool CreateScriptableObjectValidate()
        {
            return Selection.activeObject is MonoScript monoScript && IsScriptableObject(monoScript.GetClass());
        }

        private static bool IsScriptableObject(System.Type type) => typeof(ScriptableObject).IsAssignableFrom(type);
    }
}