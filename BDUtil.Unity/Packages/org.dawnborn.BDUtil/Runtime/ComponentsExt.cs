using System;
using UnityEngine;

namespace BDUtil
{
    public static class ComponentsExt
    {
        /// Like send-message, but interface-based, not string-and-reflection based.
        public static void Broadcast<T>(this Component thiz, Action<T> action, Predicate<T> only = default)
        {
            only ??= t => true;
            foreach (T t in thiz.GetComponents<T>())
            {
                if (!only(t)) continue;
                action(t);
            }
        }
        /// Like send-message, but interface-based, not string-and-reflection based.
        public static void BroadcastChildren<T>(this Component thiz, Action<T> action, Predicate<T> only = default)
        {
            only ??= t => true;
            foreach (T t in thiz.GetComponentsInChildren<T>())
            {
                if (!only(t)) continue;
                action(t);
            }
        }
        /// Like send-message, but interface-based, not string-and-reflection based.
        public static void BroadcastParents<T>(this Component thiz, Action<T> action, Predicate<T> only = default)
        {
            only ??= t => true;
            foreach (T t in thiz.GetComponentsInParent<T>())
            {
                if (!only(t)) continue;
                action(t);
            }
        }
        /// Destroy transform children in an editor-safe way.
        public static void DestroyChildren(this Transform thiz) => EditorUtils.DestroyChildrenByPlaystate(thiz);
        /// Destroy transform children in an editor-safe way.
        public static void DestroyChildren<T>(this T thiz) where T : Component => thiz.transform.DestroyChildren();
        /// Destroy transform children in an editor-safe way.
        public static void DestroyChildren(this GameObject thiz) => thiz.transform.DestroyChildren();

        /// Returns an inactive object (so it can be further customized) which is editor-linked with its prefab.
        public static GameObject CloneInactive(this GameObject thiz) => EditorUtils.CloneInactive(thiz);

        /// Returns an inactive object (so it can be further customized) which is editor-linked with its prefab.
        public static T CloneInactive<T>(this T thiz) where T : Component
        => thiz.gameObject.CloneInactive().GetComponent<T>();
    }
}
