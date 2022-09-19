using System;
using UnityEngine;

namespace BDUtil
{
    public static class GameControllers
    {
        /// Unity builtin.
        public const string Tag = "GameController";

        /// The default controller object (similar to Cameras.main).
        public static GameObject Default => Find(false);

        /// Gets the game controller tagged with `Tag`, "the" game controller.
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

        public static Component GetOrPut(Type type, string path = default, bool orAllocate = true)
        {
            GameObject controller = Find(orAllocate);
            if (controller == null) return null;
            Transform ptr = controller.transform;
            if (!path.IsEmpty()) foreach (string p in path.Split('/'))
                {
                    if (p.IsEmpty()) continue;

                    Transform child = ptr.Find(p);
                    if (child == null)
                    {
                        if (!orAllocate) return null;
                        GameObject created = new(p);
                        // TODO: Any tags etc?
                        child = created.transform;
                        child.parent = ptr;
                    }
                    ptr = child;
                }
            Component component = ptr.GetComponent(type);
            if (orAllocate && component == null) component = ptr.gameObject.AddComponent(type);
            return component;
        }

        /// Gets the named component, allocating it if absent (at/from the given path).
        public static T GetOrPut<T>(string path = default, bool orAllocate = true) where T : Component
        => (T)GetOrPut(typeof(T), path, orAllocate);
    }
}