using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil
{
    public static class GameControllers
    {
        /// Unity builtin.
        public const string Tag = "GameController";

        /// The default controller object (similar to Cameras.main).
        public static GameObject Default => Find(false);

        /// Gets the game controller tagged with `Tag`.
        public static GameObject Find(bool orAllocate = false)
        {
            GameObject controller = GameObject.FindGameObjectWithTag(Tag);
            if (controller == null && orAllocate)
            {
                controller = new GameObject("Default Controller")
                {
                    tag = Tag
                };
                // TODO: Does this need hide flags?
            }
            return controller;
        }

        /// Gets the named component from the Tagged controller (or children, or null).
        public static T Find<T>() where T : Component => Find(false)?.GetComponentInChildren<T>();

        public static Component GetOrPut(Type type, string path = default)
        {
            Transform ptr = Find(true).transform;
            if (!path.IsEmpty()) foreach (string p in path.Split('/'))
                {
                    if (p.IsEmpty()) continue;

                    Transform child = ptr.Find(p);
                    if (child == null)
                    {
                        GameObject created = new(p);
                        // TODO: Any tags etc?
                        child = created.transform;
                        child.parent = ptr;
                    }
                    ptr = child;
                }
            Component component = ptr.GetComponent(type);
            if (component == null) component = ptr.gameObject.AddComponent(type);
            return component;
        }

        /// Gets the named component, allocating it if absent (at/from the given path).
        public static T GetOrPut<T>(string path = default) where T : Component
        => (T)GetOrPut(typeof(T), path);

        // I'm not sure this makes any more sense than just overriding Awake...
        [Serializable]
        public class Ref<T> where T : Component
        {
            [Tooltip("If set: path to look for OR CREATE the linked controller")]
            [SerializeField, SuppressMessage("IDE", "IDE0044")]
            string Path;
            [NonSerialized]
            T Ptr;

            public T Get() => Ptr ??= GetOrPut<T>(Path);
        }
    }
}