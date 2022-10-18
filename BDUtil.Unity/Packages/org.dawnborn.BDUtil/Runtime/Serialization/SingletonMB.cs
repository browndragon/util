using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// A singleton monobehaviour like camera or eventsystem, storing a self-reference.
    public class SingletonMB<T> : MonoBehaviour
    where T : SingletonMB<T>
    {
        protected virtual void OnEnable()
        {
            if (_main != null && _main != this)
                Debug.LogWarning($"Somehow you had another ticker {_main.GetInstanceID()} vs {GetInstanceID()}?");
            _main = (T)this;
        }
        protected virtual void OnDisable()
        {
            if (_main != null && _main != this)
                Debug.LogWarning($"Somehow you had another ticker {_main.GetInstanceID()} vs {GetInstanceID()}?");
            _main = null;
        }

        static T _main;
        [SuppressMessage("IDE", "IDE1006")]
        public static T main => _main ? _main : (_main = FindOrCreate());
        static T FindOrCreate()
        {
            T instance = FindObjectOfType<T>();
            if (instance != null) return instance;
            GameObject go = new() { hideFlags = HideFlags.DontSave };
            DontDestroyOnLoad(go);
            return go.AddComponent<T>();
        }
    }
}