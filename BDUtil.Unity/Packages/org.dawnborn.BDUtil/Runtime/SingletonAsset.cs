using System;
using UnityEngine;

namespace BDUtil
{
    public static class SingletonAsset<T> where T : ScriptableObject
    {
        static readonly string AssetPath = $"Assets/BDUtil/{typeof(T).Name}.asset";
        static T instance;
        public static T Instance => instance ??= (T)EditorUtils.AcquireAsset(typeof(T), AssetPath);
        /// Good idea to call me with `[UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]` & `[UnityEditor.InitializeOnLoad]`
        /// But you might need to extend topic or something...
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
