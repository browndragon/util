using System;
using System.Reflection;
using UnityEngine;

namespace BDUtil.Serialization
{
    public class StaticAsset : ScriptableObject
    {
        public enum Basenames
        {
            ByT = default,
            DirIsLiteral,
        }

        /// Tag a child class with this to specify how it should save.
        /// Default is `/Assets/Resources/` (triggering the above clause ;) ).
        /// You're assumed relative to the project folder, so you'll want `/Assets/...`.
        [AttributeUsage(AttributeTargets.Class)]
        public class FilePathAttribute : Attribute
        {
            public static readonly FilePathAttribute Default = new();
            public string DirName = EditorUtils.DefaultFolder;
            public Basenames Basename;
            public FilePathAttribute(string dirName = default, Basenames basename = Basenames.ByT)
            {
                DirName = dirName ?? DirName;
                Basename = basename;
            }
            public string GetFilePath(Type type) => StaticAsset.GetFilePath(type, DirName, Basename);
        }
        public static string GetFilePath(Type type, string dirName, Basenames basename)
        {
            string path = dirName.OrThrow();
            if (path.StartsWith("Assets")) { }
            else if (path.StartsWith("/")) path = $"/Assets{path}";
            else path = $"Assets/{path}";

            switch (basename)
            {
                case Basenames.DirIsLiteral: break;
                default:
                    if (!path.EndsWith("/")) path = $"{path}/";
                    switch (basename)
                    {
                        case Basenames.ByT: path = $"{path}{type.Name}.asset"; break;
                    }
                    break;
            }
            return path;
        }
    }


    /// With deep apologies to EDITOR NAMESPACED
    // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ScriptableSingleton.cs
    public class StaticAsset<T> : StaticAsset where T : StaticAsset<T>
    {
        [Tooltip("Push this to clean the preloaded list, and ensure that THIS is the preloaded instance")]
        [SerializeField, Invoke(nameof(EnsurePreloaded))] Invoke.Button ensurePreloaded;
        internal void EnsurePreloaded() => EditorUtils.AdjustPreloadedAssets(remove => true, this);
        static T _main;
        public static T main
        {
            get
            {
                if (_main == null)
                    CreateAndLoad();
                return _main;
            }
        }
        public string Path
        {
            get
            {
                Type type = typeof(T);
                FilePathAttribute filePathAttribute = type.GetCustomAttribute<StaticAsset.FilePathAttribute>() ?? StaticAsset.FilePathAttribute.Default;
                return filePathAttribute.GetFilePath(type);
            }
        }

        // On domain reload ScriptableObject objects gets reconstructed from a backup. We therefore set the s_Instance here
        protected StaticAsset()
        {
            if (_main != null && _main != this) Debug.LogError($"{GetType().Name}.main={_main.GetInstanceID()} already exists vs this={this.GetInstanceID()}. Did you query the singleton in a constructor?");
            else
            {
                _main = this as T;
                System.Diagnostics.Debug.Assert(_main != null);
            }
        }

        private static void CreateAndLoad()
        {
            System.Diagnostics.Debug.Assert(_main == null);
            Type type = typeof(T);
            FilePathAttribute filePathAttribute = type.GetCustomAttribute<FilePathAttribute>() ?? FilePathAttribute.Default; ;
            Debug.Log($"Attempting create&load: {typeof(T)}");
            // Load
            string filePath = filePathAttribute?.GetFilePath(type);
            if (!string.IsNullOrEmpty(filePath))
            {
                T t = EditorUtils.Load<T>(filePath);
                Debug.Log($"Loaded {t?.GetInstanceID() ?? 0} from {filePath} & (if real) main is: {_main?.GetInstanceID() ?? 0}");
            }
            if (_main == null)
            {
                // Create
                T t = CreateInstance<T>();
                t.name = t.name.IsEmpty() ? $"{typeof(T).Name}.main" : t.name;
                EditorUtils.StoreNewAsset(_main, filePath);
            }

            System.Diagnostics.Debug.Assert(_main != null);
        }
    }
}