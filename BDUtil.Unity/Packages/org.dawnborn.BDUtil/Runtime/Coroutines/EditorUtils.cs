using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BDUtil
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    /// These are safe to call from runtime code; they should degrade and handle the difference between the modes.

#if UNITY_EDITOR
    // https://medium.com/@fiftytwo/fast-singleton-approach-in-unity-fdba0b5309d5
    [InitializeOnLoad]
#endif
    public static class EditorUtils
    {
        static EditorUtils()
        {
#if UNITY_EDITOR
            Debug.Log($"Preloading {PlayerSettings.GetPreloadedAssets().Summarize()}");
#endif
        }

        public const string AssetsFolder = "Assets/";
        public const string ResourcesFolder = "/Resources/";
        public const string AssetSuffix = ".asset";

        public static bool IsPlayingOrWillChangePlaymode =>
#if UNITY_EDITOR
        EditorApplication.isPlayingOrWillChangePlaymode
#else  // UNITY_EDITOR
        Application.isPlaying
#endif  // UNITY_EDITOR
        ;

        // Call during EditorApplication.delay or, if already playing, end of frame.
        public static void Delay(UnityEngine.Object guard, Action action, bool orSchedule = false)
        {
            void PlayIfNonNull() { if (guard != null) action(); }
            if (orSchedule && Application.isPlaying) { Coroutines.Schedule(PlayIfNonNull, Coroutines.End); return; }
#if UNITY_EDITOR
            EditorApplication.delayCall += PlayIfNonNull;
            return;
#else  // UNITY_EDITOR
            throw new NotSupportedException($"Can't delay {action} while not playing in runtime build");
#endif  // UNITY_EDITOR
        }

        public static T InstantiateWithLink<T>(T go) where T : UnityEngine.Object =>
#if UNITY_EDITOR
            (T)PrefabUtility.InstantiatePrefab(go)
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
        public static UnityEngine.Object AcquireAsset(Type t, string assetPath = default)
        {
#if UNITY_EDITOR
            assetPath ??= $"{AssetsFolder}{ResourcesFolder}{t.Name}{AssetSuffix}";
            var instance = Load(assetPath, t, false);

            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance(t);
                string directoryPath = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
            }

            // Changing the preloaded assets is only effective if the editor is not in play mode
            if (!IsPlayingOrWillChangePlaymode) InsertPreloadedAsset(instance);

            return instance;
#else
            throw new NotSupportedException($"Can't acquire {t} @ {assetPath} at runtime!");
#endif
        }

        public static void InsertPreloadedAsset(UnityEngine.Object asset = default)
        {
#if UNITY_EDITOR
            var newPreloadedAssets = new List<UnityEngine.Object>();
            int instanceId = asset?.GetInstanceID() ?? 0;
            UnityEngine.Object[] hadPreloadedAssets = PlayerSettings.GetPreloadedAssets();
            foreach (UnityEngine.Object @object in hadPreloadedAssets)
            {
                if (!@object) continue;
                newPreloadedAssets.Add(@object);
                if (@object.GetInstanceID() == instanceId) instanceId = 0;
            }
            if (instanceId != 0 || newPreloadedAssets.Count != hadPreloadedAssets.Length)
            {
                if (instanceId != 0) newPreloadedAssets.Add(asset);
                PlayerSettings.SetPreloadedAssets(newPreloadedAssets.ToArray());
                AssetDatabase.SaveAssets();
            }
#endif  // UNITY_EDITOR
        }
        public static void RemoveEmptyPreloadedAssets() => InsertPreloadedAsset(null);

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
                    AssetDatabase.CreateFolder(accumulatedUnityFolder, folder);
                accumulatedSystemFolder += "/" + folder;
                accumulatedUnityFolder += "/" + folder;
            }

            AssetDatabase.CreateAsset(newAsset, AssetsFolder + ParentFolder + "/" + AssetName + ".asset");
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
        public interface IClone { string PrefabRef { get; } }
        public static string GetAssetPath(UnityEngine.Object @object) => @object switch
        {
            null => null,
            IClone c => c.PrefabRef,
            GameObject g => g.GetComponent<IClone>()?.PrefabRef ?? GetAssetPathOrNull(g),
            Component c => c.GetComponent<IClone>()?.PrefabRef ?? GetAssetPathOrNull(c.gameObject),
            _ => GetAssetPathOrNull(@object),
        };

        public static UnityEngine.Object Load(string assetPath, Type loadType, bool logWarning = true)
        {
            if (assetPath.IsEmpty()) return null;

            if (loadType == null) return null;
            Type getComponent = null;
            if (typeof(Component).IsAssignableFrom(loadType))
            {
                getComponent = loadType;
                loadType = typeof(GameObject);
            }

            string resourcesPath = GetResourcesPath(assetPath);

            if (!resourcesPath.IsEmpty())
            {
                var resource = Resources.Load(resourcesPath, loadType);
                return getComponent == null ? resource : ((GameObject)resource).GetComponent(getComponent);
            }

            // TODO: asset bundles etc too?
#if UNITY_EDITOR
            if (logWarning) Debug.LogWarning($"Trying to load `{assetPath}` from AssetDatabase; Editor Only!!!");
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, loadType);
            return getComponent == null ? asset : ((GameObject)asset).GetComponent(getComponent);
#else  // UNITY_EDITOR
            if (logWarning) Debug.LogWarning($"Couldn't load `{assetPath}`");
            return null;
#endif  // UNITY_EDITOR
        }

        /// Loads an e.g. prefab based on its assetpath.
        public static T Load<T>(string assetPath) where T : UnityEngine.Object
        => (T)Load(assetPath, typeof(T));
    }
}