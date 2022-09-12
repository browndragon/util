using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil
{
    [Serializable]
    public class SingletonAsset : ScriptableObject
    {
        static readonly Dictionary<Type, SingletonAsset> singletons = new();
        public static SingletonAsset GetSingletonAsset(Type type) => singletons.GetValueOrDefault(type);
        public static T GetSingletonAsset<T>() where T : SingletonAsset => (T)GetSingletonAsset(typeof(T));

        protected virtual void OnEnable()
        {
            if (EditorUtils.IsPlayingOrWillChangePlaymode) singletons[GetType()] = this;
            else EditorUtils.InsertPreloadedAsset(this);
        }
        protected virtual void OnDisable()
        {
            if (!EditorUtils.IsPlayingOrWillChangePlaymode) EditorUtils.RemoveEmptyPreloadedAssets();
        }
    }
    public class SingletonAsset<T> : SingletonAsset
    where T : SingletonAsset<T>
    {
        public static T _main;
        public static T main => _main ??= GetSingletonAsset<T>() ?? CreateInstance<T>() /* which registers in OnEnable... */;
    }
}
