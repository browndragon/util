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
        [SerializeField] Invokable.Layout buttons;

        [Tooltip("Push this to clean the preloaded list, and ensure that THIS is the preloaded instance")]
        [Invokable]
        internal void EnsurePreloaded()
        {
            EditorUtils.AdjustPreloadedAssets(remove => true, this);
            Debug.Log($"Added {this} to preloaded assets (you should stop seeing duplicate static asset errors!)");
        }
        static T _main;
        public static T main
        {
            get
            {
                if (_main == null)
                    LoadOrCreate();
                return _main;
            }
        }
        // On domain reload ScriptableObject objects gets reconstructed from a backup. We therefore set the s_Instance here
        protected StaticAsset()
        {
            if (_main != null && _main != this) Debug.LogError($"Duplicate {GetType().Name}.main={_main.GetInstanceID()} already exists vs this={this.GetInstanceID()}");
            else
            {
                _main = this as T;
                System.Diagnostics.Debug.Assert(_main != null);
            }
        }

        static void LoadOrCreate()
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
                T t = (T)EditorUtils.CreateScriptableObjectOfType(typeof(T), true);
                System.Diagnostics.Debug.Assert(t != null && _main == t);
            }
            System.Diagnostics.Debug.Assert(_main != null);
        }
    }
}