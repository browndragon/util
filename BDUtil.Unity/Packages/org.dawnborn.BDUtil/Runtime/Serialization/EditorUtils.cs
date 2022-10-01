using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BDUtil.Fluent;
using UnityEngine;

namespace BDUtil.Serialization
{
    using System.Diagnostics.CodeAnalysis;
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

        static bool IsSO(Type type) => type != null && typeof(ScriptableObject).IsAssignableFrom(type);
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
            // OTHERWISE, If we've got a useful ProjectWindow, use that.
            string projectWindow = ProjectWindowPath;
            if (!projectWindow.IsEmpty()) return StaticAsset.GetFilePath(type, projectWindow, StaticAsset.Basenames.ByT);

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
                string dir = Path.GetDirectoryName(path);
                ;
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    int prevSlash = dir.IndexOf("/"), slash = dir.IndexOf("/", prevSlash + 1);
                    while (slash >= 0)
                    {
                        if (!AssetDatabase.IsValidFolder(dir[0..slash]))
                        {
                            string pre = dir[0..prevSlash];
                            string post = dir[(prevSlash + 1)..slash];
                            Debug.Log($"Creating {pre} + {post}");
                            AssetDatabase.CreateFolder(pre, post);
                        }
                        prevSlash = slash;
                        slash = dir.IndexOf("/", prevSlash + 1);
                    }
                    if (!dir.EndsWith("/"))
                    {
                        string pre = dir[0..prevSlash];
                        string post = dir[(prevSlash + 1)..];
                        Debug.Log($"Creating final {pre} + {post}");
                        AssetDatabase.CreateFolder(pre, post);
                    }
                }
                if (!AssetDatabase.IsValidFolder(dir)) Debug.Log($"WTF see above?!");
                Debug.Log($"Trying to create {asset} at {path}");
                ProjectWindowUtil.CreateAsset(asset, path);
                return asset;
#else
                asset.name = $"{type.Name}.{asset.GetInstanceID()}";
                Debug.LogWarning($"Created runtime-only {asset}; can't store", asset);
#endif
            }
            return asset;
        }
    }
}