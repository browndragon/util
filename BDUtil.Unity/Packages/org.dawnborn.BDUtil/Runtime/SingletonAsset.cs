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

        public bool IsEnabled { get; private set; }

        protected void OnEnable()
        {
            if (EditorUtils.IsPlayingOrWillChangePlaymode) singletons[GetType()] = this;
            else EditorUtils.InsertPreloadedAsset(this);
            TryEnableSubsystem();
        }
        protected void OnDisable()
        {
            if (IsEnabled) OnDisableSubsystem();
            if (!EditorUtils.IsPlayingOrWillChangePlaymode) EditorUtils.RemoveEmptyPreloadedAssets();
        }
        protected void TryEnableSubsystem()
        {
            if (!Application.isPlaying) return;
            if (IsEnabled) return;
            OnEnableSubsystem();
        }
        protected virtual void OnEnableSubsystem() => IsEnabled = true;
        protected virtual void OnDisableSubsystem() => IsEnabled = false;

        // Workaround for:
        // https://issuetracker.unity3d.com/issues/application-dot-isplaying-is-false-when-onenable-is-called-from-a-scriptableobject-after-entering-the-play-mode
        [SuppressMessage("IDE", "IDE0051")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void SubsystemRegistration()
        { foreach (SingletonAsset asset in singletons.Values) asset.TryEnableSubsystem(); }
    }
    public class SingletonAsset<T> : SingletonAsset
    where T : SingletonAsset<T>
    {
        public static T _main;
        [SuppressMessage("IDE", "IDE1006")]

        public static T main => _main ??= GetSingletonAsset<T>() ?? CreateInstance<T>() /* which registers in OnEnable... */;
    }
}
