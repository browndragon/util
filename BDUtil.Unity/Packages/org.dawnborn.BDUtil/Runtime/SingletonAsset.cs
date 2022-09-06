using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BDUtil
{
    public static class SingletonAsset
    {
        public static string Infix = "BDUtil";
        public static string GetAssetPathForType(Type t, string Infix = default)
        => $"Assets/{Infix ?? SingletonAsset.Infix}/{t.Name}.asset";
        public static UnityEngine.Object AcquireAsset(Type t, string assetPath = default)
        => EditorUtils.AcquireAsset(t, assetPath ?? GetAssetPathForType(t));

    }
    /// Good idea to call me with `[UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]` & `[UnityEditor.InitializeOnLoad]`
    /// But you might need to extend topic or something...
    public static class SingletonAsset<T> where T : ScriptableObject
    {
        static readonly string assetPath = SingletonAsset.GetAssetPathForType(typeof(T));
        static T instance;
        public static T Instance => instance ??= (T)SingletonAsset.AcquireAsset(typeof(T), assetPath);
        /// Provides a utility for the actual scriptableobject's enable method.
        public static T SetIfUnset(T thiz)
        {
            if (instance == null) instance = thiz;
            else if (instance != thiz) Debug.LogWarning(
                $"Singleton {typeof(T)} has loaded two instances {instance.GetInstanceID()} != {thiz.GetInstanceID()}!"
            );
            return instance;
        }
    }
}
