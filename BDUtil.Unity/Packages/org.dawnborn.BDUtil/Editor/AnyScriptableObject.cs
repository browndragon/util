using UnityEditor;
using UnityEngine;

namespace BDUtil
{
    public static class AnyScriptableObject
    {
        [MenuItem("Assets/Create/BDUtil/Script Asset", false, 0)]
        public static void CreateScriptableObject()
        {
            var selection = Selection.activeObject;
            var assetPath = AssetDatabase.GetAssetPath(selection.GetInstanceID());
            var path = $"{System.IO.Directory.GetParent(assetPath)}/{selection.name}.asset";
            ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance(selection.name), path);
        }

        [MenuItem("Assets/Create/BDUtil/Script Asset", true)]
        public static bool CreateScriptableObjectValidate()
        {
            return Selection.activeObject is MonoScript monoScript && IsScriptableObject(monoScript.GetClass());
        }

        private static bool IsScriptableObject(System.Type type) => typeof(ScriptableObject).IsAssignableFrom(type);
    }
}