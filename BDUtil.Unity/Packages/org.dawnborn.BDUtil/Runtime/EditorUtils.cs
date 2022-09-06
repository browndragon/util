using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BDUtil
{
    public static class EditorUtils
    {
        public static T InstantiateWithLink<T>(T go) where T : UnityEngine.Object =>
#if UNITY_EDITOR
            (T)UnityEditor.PrefabUtility.InstantiatePrefab(go)
#else  // !UNITY_EDITOR
            UnityEngine.Object.Instantiate(go)
#endif  // UNITY_EDITOR
        ;
        public static GameObject CloneInactive(GameObject gameObject)
        {
            bool wasActive = gameObject.activeSelf;
            try
            {
                gameObject.SetActive(false);
                return InstantiateWithLink(gameObject);
            }
            finally
            {
                gameObject.SetActive(wasActive);
            }
        }
        public static void DestroyChildrenByPlaystate(Transform transform)
        {
            if (!Application.isPlaying) for (int i = transform.childCount - 1; i >= 0; --i) UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
            else for (int i = transform.childCount - 1; i >= 0; --i) UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
        }

        public static UnityEngine.Object AcquireAsset(Type t, string assetPath)
        {
#if UNITY_EDITOR
            assetPath.OrThrowInternal();
            var instance = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, t);

            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance(t);
                string directoryPath = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                UnityEditor.AssetDatabase.Refresh();
                UnityEditor.AssetDatabase.CreateAsset(instance, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
            }

            // Changing the preloaded assets is only effective if the editor is not in play mode
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                var preloadedAssets = new List<UnityEngine.Object>(UnityEditor.PlayerSettings.GetPreloadedAssets());
                if (!preloadedAssets.Contains(instance))
                {
                    preloadedAssets.Add(instance);
                    UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
            return instance;
#else
            return null;
#endif
        }

        public static void CreateAssetInFolder(UnityEngine.Object newAsset, string ParentFolder, string AssetName)
        {
#if UNITY_EDITOR
            string[] pathSegments = ParentFolder.Split(new char[] { '/' });
            string accumulatedUnityFolder = "Assets";
            string accumulatedSystemFolder = Application.dataPath + System.IO.Path.GetDirectoryName("Assets");
            foreach (string folder in pathSegments)
            {
                // TODO: Apparently very unsafe -- meaning so is the ^^^ singleton-supporting code?
                if (!System.IO.Directory.Exists(accumulatedSystemFolder + System.IO.Path.GetDirectoryName(accumulatedUnityFolder + "/" + folder)))
                    UnityEditor.AssetDatabase.CreateFolder(accumulatedUnityFolder, folder);
                accumulatedSystemFolder += "/" + folder;
                accumulatedUnityFolder += "/" + folder;
            }

            UnityEditor.AssetDatabase.CreateAsset(newAsset, "Assets/" + ParentFolder + "/" + AssetName + ".asset");
#endif
        }
    }
}