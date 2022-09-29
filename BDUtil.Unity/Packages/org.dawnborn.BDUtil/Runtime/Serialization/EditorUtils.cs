using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BDUtil.Serialization
{
    using System.Reflection;
    using BDUtil.Math;
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
        public const string ResourcesFolder = "Resources/";
        public static string DefaultFolder = $"{AssetsFolder}{ResourcesFolder}";

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

        /// Instantiates a prefab with a link back to the parent.
        /// Mostly for editor scripts, since there's no such beast @ real runtime.
        public static T InstantiateWithLink<T>(T go) where T : UnityEngine.Object =>
#if UNITY_EDITOR
            (T)PrefabUtility.InstantiatePrefab(go)
#else  // !UNITY_EDITOR
            UnityEngine.Object.Instantiate(go)
#endif  // UNITY_EDITOR
        ;
        public interface ICloned { GameObject gameObject { get; } GameObject Root { get; } }
        /// Support for the CloneTag type. At runtime, all it can do is examine the CloneTag instance,
        /// but in the editor it can also examine the PrefabUtility and see what it says;
        /// CloneTag uses _this_ to ensure it's correct.
        public static GameObject GetCloneRoot(ICloned cloneInstance)
        {
            if (cloneInstance == null)
            {
                Debug.Log($"Got null cloneTag {cloneInstance} somehow?!");
                return null;
            }
            if (!cloneInstance.gameObject.scene.IsValid())
            {
                Debug.Log($"Got cloneTag {cloneInstance} from no-scene somehow?! (asset?)");
                return cloneInstance.Root;
            }
            GameObject bestRoot = cloneInstance.Root;
#if UNITY_EDITOR
            bestRoot = PrefabUtility.GetCorrespondingObjectFromSource(cloneInstance.gameObject);
            if (bestRoot == null)
            {
                Debug.LogWarning($"Encountered awake clone with no parents! {cloneInstance}(.root={cloneInstance.Root}, .bestRoot={bestRoot})");
                return null;
            }
            if (bestRoot == cloneInstance.gameObject)
            {
                Debug.LogWarning($"Snake eating own tail! {cloneInstance}(.root={cloneInstance.Root}, .bestRoot={bestRoot})");
                return null;
            }
            if (cloneInstance.Root != null && cloneInstance.Root != bestRoot)
            {
                Debug.LogWarning($"Prefab & CloneTag disagree (going with prefab)! {cloneInstance}(.root={cloneInstance.Root}, .bestRoot={bestRoot})");
                return bestRoot;
            }
#endif
            // At runtime, we just have to hope that we've wired all of the other state up correctly.
            return bestRoot;
        }

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
        public static string AssetNaiveBasename(UnityEngine.Object o) => o switch
        {
            null => null,
            GameObject => $"{o.name}.prefab",
            Component => $"{o.name}.prefab",
            _ => $"{o.name}.asset",
        };

        /// Easy filtering of the preloaded asset list.
        /// All nulls, as well as anything of T & matching the predicate, will be removed.
        /// All non-T or failing the predicate will be retained.
        /// Then the @new element will be appended (no predicate)
        /// However, in play mode... none of this happens at all.
        public static void AdjustPreloadedAssets<T>(Func<T, bool> removePredicate, T @new = default)
        where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            UnityEngine.Object[] hadPreloadedAssets = PlayerSettings.GetPreloadedAssets();
            var newPreloadedAssets = new List<UnityEngine.Object>();
            foreach (UnityEngine.Object @object in hadPreloadedAssets)
            {
                if (@object == null) continue;
                if (@object is not T t) { newPreloadedAssets.Add(@object); continue; }
                if (!removePredicate(t)) newPreloadedAssets.Add(t);
            }
            if (@new != null) newPreloadedAssets.Add(@new);
            PlayerSettings.SetPreloadedAssets(newPreloadedAssets.ToArray());
            AssetDatabase.SaveAssets();
#endif  // UNITY_EDITOR
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

        public static UnityEngine.Object Load(string assetPath, Type loadType, bool logWarning = true)
        {
            if (loadType == null) return null;
            Type getComponent = null;
            if (typeof(Component).IsAssignableFrom(loadType))
            {
                getComponent = loadType;
                loadType = typeof(GameObject);
            }
            if (assetPath.IsEmpty()) assetPath = GuessAssetPath(loadType);

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

        public static bool IsSO(Type type) => type != null && typeof(ScriptableObject).IsAssignableFrom(type);
        public static string GuessAssetPath(Type type)
        {
            if (type == null) return null;
            StaticAsset.FilePathAttribute fpA = type.GetCustomAttribute<StaticAsset.FilePathAttribute>();
            if (typeof(StaticAsset).IsAssignableFrom(type)) fpA ??= StaticAsset.FilePathAttribute.Default;
            // First try the "proper location" (if it has one).
            // This catches e.g. singletons, which are precise about naming.
            if (fpA != null) return fpA.GetFilePath(type);

            // Otherwise, if we have a selection (like a script), place it nearby
            if (Selection.activeObject != null)
            {
                foreach (string guid in Selection.assetGUIDs ?? Array.Empty<string>())
                {
                    string selected = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(selected)) continue;
                    string dir = Path.GetDirectoryName(selected);
                    string asset = Path.Combine(dir, $"{type.Name}.asset");
                    return asset;
                }
            }
            // If we've got a useful ProjectWindow, use that.
            string projectWindow = ProjectWindowPath;
            if (!projectWindow.IsEmpty()) return projectWindow;

            // Finally, stick it in the default location.
            return StaticAsset.FilePathAttribute.Default.GetFilePath(type);
        }
        static readonly Type projectWindowUtilType = typeof(ProjectWindowUtil);
        // SIGH
        static readonly MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
        public static string ProjectWindowPath => getActiveFolderPath.Invoke(null, Array.Empty<object>())?.ToString();

        /// Creates an asset where it wants to go, in your current context, or else in the default location.
        public static ScriptableObject CreateScriptableObjectOfType(Type type, bool store)
        {
            if (!IsSO(type)) return null;
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            if (store)
            {
#if UNITY_EDITOR
                string path = GuessAssetPath(type).OrThrow();
                ProjectWindowUtil.CreateAsset(asset, path);
                Debug.Log($"Created {asset} wanted {path} got {AssetDatabase.GetAssetOrScenePath(asset)}");
                return asset;
#else
                asset.name = $"{type.Name}.{asset.GetInstanceID()}";
                Debug.LogWarning($"Created runtime-only {asset}; can't store", asset);
#endif
            }
            return asset;
        }

        //         public static void StoreNewAsset(UnityEngine.Object o, string assetPath = null)
        //         {
        // #if UNITY_EDITOR
        //             assetPath ??= Path.Join(DefaultFolder, AssetNaiveBasename(o));
        //             string[] parts = assetPath.Split(Path.DirectorySeparatorChar);
        //             Debug.Log($"Attempting to store {o} @ {assetPath} = {parts.Length} components");
        //             string path = parts[0];
        //             for (int i = 1; i < parts.Length - 1; ++i)
        //             {
        //                 string nextPath = Path.Join(path, parts[i]);
        //                 if (!AssetDatabase.IsValidFolder(nextPath))
        //                 {
        //                     Debug.Log($"Creating {nextPath}");
        //                     AssetDatabase.CreateFolder(path, parts[i]);
        //                 }
        //                 else
        //                 {
        //                     Debug.Log($"Had {nextPath}");
        //                 }
        //                 path = nextPath;
        //             }
        //             assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        //             try
        //             {
        //                 AssetDatabase.CreateAsset(o, assetPath);
        //             }
        //             catch (Exception e)
        //             {
        //                 Debug.LogWarning($"Can't store {o} @ {assetPath}: {e}");
        //             }
        //             AssetDatabase.SaveAssets();
        //             AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        //             EditorGUIUtility.PingObject(o);
        // #else
        //             Debug.LogWarning($"Can't store {o} @ {assetPath} at runtime!");
        // #endif
        //         }
    }
}