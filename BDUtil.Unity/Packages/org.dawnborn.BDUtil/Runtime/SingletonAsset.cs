using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil
{
    // If your singleton needs access to unity ticks, see Ticker.
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

        // Workaround for:
        // https://issuetracker.unity3d.com/issues/application-dot-isplaying-is-false-when-onenable-is-called-from-a-scriptableobject-after-entering-the-play-mode
        protected virtual void OnRuntimeInitialize(RuntimeInitializeLoadType type) { }

        static void SendOnRuntimeInitialize(RuntimeInitializeLoadType type)
        { foreach (SingletonAsset asset in singletons.Values) asset.OnRuntimeInitialize(type); }

        [SuppressMessage("IDE", "IDE0051")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void AfterAssembliesLoaded() => SendOnRuntimeInitialize(RuntimeInitializeLoadType.AfterAssembliesLoaded);
        [SuppressMessage("IDE", "IDE0051")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoad() => SendOnRuntimeInitialize(RuntimeInitializeLoadType.AfterSceneLoad);
        [SuppressMessage("IDE", "IDE0051")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad() => SendOnRuntimeInitialize(RuntimeInitializeLoadType.BeforeSceneLoad);
        [SuppressMessage("IDE", "IDE0051")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void BeforeSplashScreen() => SendOnRuntimeInitialize(RuntimeInitializeLoadType.BeforeSplashScreen);
        [SuppressMessage("IDE", "IDE0051")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void SubsystemRegistration() => SendOnRuntimeInitialize(RuntimeInitializeLoadType.SubsystemRegistration);
    }
    public class SingletonAsset<T> : SingletonAsset
    where T : SingletonAsset<T>
    {
        public static T _main;
        public static T main => _main ??= GetSingletonAsset<T>() ?? CreateInstance<T>() /* which registers in OnEnable... */;
    }
}
