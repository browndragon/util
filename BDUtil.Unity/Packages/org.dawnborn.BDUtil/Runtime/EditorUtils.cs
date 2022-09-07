using System;
using System.Collections.Generic;
using System.IO;
using BDUtil.Serialization;
using UnityEditor;
using UnityEngine;

namespace BDUtil
{
    public static class EditorUtils
    {
        public const string AssetsFolder = "Assets/";
        public const string ResourcesFolder = "/Resources/";

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

        /// Load or, if can't be loaded, _create_, an asset of the given type at the given path.
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
            Debug.Log($"Can't acquire {t} @ {assetPath} at runtime!");
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

            UnityEditor.AssetDatabase.CreateAsset(newAsset, AssetsFolder + ParentFolder + "/" + AssetName + ".asset");
#else  // UNITY_EDITOR
            Debug.Log($"Can't create missing {newAsset} @ {ParentFolder}/{AssetName} at runtime!");
#endif  // UNITY_EDITOR
        }

        /// Translate a "Assets/MyProject/Resources/MyStage/MyAsset.prefab" -> "MyStage/MyAsset" for loading as a resource.
        public static string GetResourcesPath(string assetPath)
        {
            if (assetPath.IsEmpty()) return null;
            int resourceIndex = assetPath.LastIndexOf(ResourcesFolder);
            if (resourceIndex < 0) return null;
            resourceIndex += ResourcesFolder.Length;
            int lastDot = assetPath.LastIndexOf('.');
            if (lastDot < 0) lastDot = assetPath.Length;
            string resourcesPath = assetPath[resourceIndex..lastDot];
            return resourcesPath;
        }

        static string GetAssetPathOrNull(UnityEngine.Object @object)
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(@object);
#else  // UNITY_EDITOR
            Debug.LogWarning($"Can't get {@object} path at runtime!!!");
            return null;
#endif
        }
        public static string GetAssetPath(UnityEngine.Object @object) => @object switch
        {
            null => null,
            Clone c => c.PrefabRef,
            GameObject g => g.GetComponent<Clone>()?.PrefabRef ?? GetAssetPathOrNull(g),
            Component c => c.GetComponent<Clone>()?.PrefabRef ?? GetAssetPathOrNull(c.gameObject),
            ScriptableObject s => GetAssetPathOrNull(s),
            _ => null,
        };

        static T Cast<T>(UnityEngine.Object o)
        where T : UnityEngine.Object => (T)o;
        static T GetComponent<T>(UnityEngine.Object o)
        where T : UnityEngine.Object => ((GameObject)o)?.GetComponent<T>();

        /// Loads an e.g. prefab based on its assetpath.
        public static T Load<T>(string assetPath) where T : UnityEngine.Object
        {
            if (assetPath.IsEmpty()) return null;
            string resourcesPath = GetResourcesPath(assetPath);

            Type loadType = typeof(T);
            Func<UnityEngine.Object, T> cast = Cast<T>;
            if (typeof(Component).IsAssignableFrom(loadType))
            {
                loadType = typeof(GameObject);
                cast = GetComponent<T>;
            }

            if (!resourcesPath.IsEmpty()) return cast(Resources.Load(resourcesPath, loadType));
#if UNITY_EDITOR
            Debug.LogWarning($"Trying to load `{assetPath}` from AssetDatabase; Editor Only!!!");
            // We could not find it in resources so we just try the AssetDatabase.
            return cast(UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, loadType));
#else  // UNITY_EDITOR
            return null;
#endif  // UNITY_EDITOR
        }
    }
}