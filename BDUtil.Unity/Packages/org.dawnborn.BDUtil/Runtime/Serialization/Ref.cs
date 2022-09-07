using System;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// Holds a string-based path to a serialized reference.
    /// Obviously this will break with e.g. renames!
    [Serializable]
    public struct Ref<T> : IEquatable<Ref<T>>
    where T : UnityEngine.Object
    {
        [SerializeField] string assetPath;
        public string AssetPath => assetPath;
        public string ResourcePath => EditorUtils.GetResourcesPath(assetPath);
        public Ref(string assetPath) => this.assetPath = assetPath;

        public static implicit operator Ref<UnityEngine.Object>(Ref<T> thiz) => new() { assetPath = thiz.assetPath };
        public static implicit operator Ref<T>(Ref<UnityEngine.Object> thiz) => (Ref<T>)thiz.Load();
        public static implicit operator string(Ref<T> thiz) => thiz.AssetPath;
        public static implicit operator Ref<T>(string path) => new() { assetPath = path };
        public static explicit operator T(Ref<T> thiz) => thiz.Load();
        public static explicit operator Ref<T>(T other)
        {
            Ref<T> thiz = new();
            thiz.SetInstance(other);
            return thiz;
        }

        public T Load() => EditorUtils.Load<T>(assetPath);
        public void SetInstance(T other) => assetPath = EditorUtils.GetAssetPath(other);

        public bool Equals(Ref<T> other) => assetPath == other.assetPath;
        public override bool Equals(object other) => other is Ref<T> @ref && Equals(@ref);
        public override int GetHashCode() => assetPath?.GetHashCode() ?? 0;
    }
}