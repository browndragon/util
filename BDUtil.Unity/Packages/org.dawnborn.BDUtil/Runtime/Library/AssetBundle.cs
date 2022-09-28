using System;
using System.Collections.Generic;
using System.Linq;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    [Tooltip("A bundle of ")]
    [CreateAssetMenu(menuName = "BDUtil/Library/Statics")]
    public class AssetBundle : ScriptableObject
    {
        static AssetBundle _main;
        /// The real main bundle (no really).
        public static AssetBundle main => _main ??= CreateMainInstance();

        [SerializeField]
        [Invoke(nameof(BecomeMainAssetBundle), Suppresses = Invoke.Suppress.Play)]
        [Tooltip("Set this as the preferred asset bundle")]
        Invoke.Button becomeMainAssetBundle;
        [SerializeField] bool isMainAssetBundle;

        [SerializeField, Expandable] List<UnityEngine.Object> Objects = new();
        readonly Dictionary<Type, Dictionary<string, UnityEngine.Object>> ByTypeByName = new();

        public UnityEngine.Object GetDefault(Type type)
        {
            if (!ByTypeByName.TryGetValue(type, out var byName))
            {
                ByTypeByName[type] = null;
                return null;
            }
            if (byName == null) return null;
            return byName.First().Value;
        }
        public UnityEngine.Object GetNamed(Type type, string name)
        {
            if (!ByTypeByName.TryGetValue(type, out var byName))
            {
                ByTypeByName[type] = null;
                return null;
            }
            if (byName == null) return null;
            return byName.GetValueOrDefault(name.OrThrow());
        }
        public T GetDefault<T>() => (T)(object)GetDefault(typeof(T));
        public T GetNamed<T>(string name) => (T)(object)GetNamed(typeof(T), name);
        /// "theirs" is the one to insert if we had null; if we didn't have null and it's not theirs, we'll throw.
        public T GetOrInsert<T>(T theirs = default, string name = default)
        where T : UnityEngine.Object
        {
            name ??= theirs?.name;
            Type type = typeof(T);
            T mine = GetNamed<T>(name);
            if (theirs != null && mine != null && theirs != mine) throw new ArgumentException($"You had {theirs} while I had {type}=>{mine}; delete one?");
            if (mine != null) return mine;
            if (theirs != null)
            {
                AddIndex(theirs);
                return theirs;
            }
            return null;
        }
        /// If ours & theirs & not equal, throw; otherwise, use ours or else theirs or else create one.
        public T GetOrCreate<T>(T theirs = default, string name = default)
        where T : ScriptableObject
        {
            name ??= theirs?.name;
            T got = GetOrInsert(theirs, name);
            if (got != null) return got;
            got = CreateInstance<T>();
            got.name = name;
            AddIndex(got);
            Debug.Log($"Creating new asset \"{name}\" {typeof(T)}=>{got} dynamically");
            return got;
        }
        void AddIndex(UnityEngine.Object @object)
        {
            if (@object == null) throw new ArgumentException($"Somehow null during construction");
            string name = @object.name;
            Type selftype = @object.GetType();
            for (Type type = selftype; type != null; type = type.BaseType)
            {
                Dictionary<string, UnityEngine.Object> byNames = ByTypeByName.GetValueOrDefault(type);
                if (byNames == null) byNames = ByTypeByName[type] = new();
                byNames[name] = @object;
            }
            foreach (Type type in selftype.GetInterfaces())
            {
                Dictionary<string, UnityEngine.Object> byNames = ByTypeByName.GetValueOrDefault(type);
                if (byNames == null) byNames = ByTypeByName[type] = new();
                byNames[name] = @object;
            }
        }
        void Reindex()
        {
            ByTypeByName.Clear();
            for (int i = Objects.Count - 1; i >= 0; --i)
            {
                if (Objects[i] == null) Objects.RemoveAt(i);
            }
            foreach (UnityEngine.Object @object in Objects) AddIndex(@object);
        }
        void OnEnable()
        {
            if (isMainAssetBundle && _main != null && _main != this) throw new NotSupportedException($">1 main asset bundles {_main} vs {this}!");
            _main = this;
            Reindex();
        }
        void OnDisable()
        {
            if (isMainAssetBundle)
            {
                if (_main != null && _main != this) Debug.Log($"{_main} != {this}; delete one?");
                else _main = null;
            }
            ByTypeByName.Clear();
        }

        static AssetBundle CreateMainInstance()
        {
            AssetBundle @new = CreateInstance<AssetBundle>();
            @new.BecomeMainAssetBundle();
            return @new;
        }
        protected void BecomeMainAssetBundle()
        {
            if (isMainAssetBundle) { _main = this; return; }
            isMainAssetBundle = true;
            EditorUtils.AdjustPreloadedAssets((AssetBundle ab) =>
            {
                ab.isMainAssetBundle = false;
                return true;
            }, this);
            _main = this;
        }
    }
}