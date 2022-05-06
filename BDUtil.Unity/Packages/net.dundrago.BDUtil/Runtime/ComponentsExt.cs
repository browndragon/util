using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil
{
    /// Extensions for fetching components at, up, and down the hierarchy.
    public enum ComponentsIn { Self = default, InParent, InChildren }
    public static class ComponentsExt
    {
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static Component GetComponentFrom(this ComponentsIn thiz, GameObject gameObject, Type type) => thiz switch
        {
            ComponentsIn.InChildren => gameObject.GetComponentInChildren(type),
            ComponentsIn.InParent => gameObject.GetComponentInParent(type),
            ComponentsIn.Self => gameObject.GetComponent(type),
            _ => throw new ArgumentException($"Unrecognized {thiz}"),
        };
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static T GetComponentFrom<T>(this ComponentsIn thiz, GameObject gameObject) => thiz switch
        {
            ComponentsIn.InChildren => gameObject.GetComponentInChildren<T>(),
            ComponentsIn.InParent => gameObject.GetComponentInParent<T>(),
            ComponentsIn.Self => gameObject.GetComponent<T>(),
            _ => throw new ArgumentException($"Unrecognized {thiz}"),
        };
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static Component[] GetComponentsFrom(this ComponentsIn thiz, GameObject gameObject, Type type) => thiz switch
        {
            ComponentsIn.InChildren => gameObject.GetComponentsInChildren(type),
            ComponentsIn.InParent => gameObject.GetComponentsInParent(type),
            ComponentsIn.Self => gameObject.GetComponents(type),
            _ => throw new ArgumentException($"Unrecognized {thiz}"),
        };
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static T[] GetComponentsFrom<T>(this ComponentsIn thiz, GameObject gameObject) => thiz switch
        {
            ComponentsIn.InChildren => gameObject.GetComponentsInChildren<T>(),
            ComponentsIn.InParent => gameObject.GetComponentsInParent<T>(),
            ComponentsIn.Self => gameObject.GetComponents<T>(),
            _ => throw new ArgumentException($"Unrecognized {thiz}"),
        };

        /// Extensions for fetching components at, up, and down the hierarchy.
        public static T GetComponent<T>(this GameObject thiz, ComponentsIn componentsIn) => componentsIn.GetComponentFrom<T>(thiz);
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static T GetComponent<T>(this Component thiz, ComponentsIn componentsIn) => componentsIn.GetComponentFrom<T>(thiz.gameObject);
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static T[] GetComponents<T>(this GameObject thiz, ComponentsIn componentsIn) => componentsIn.GetComponentsFrom<T>(thiz);
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static T[] GetComponents<T>(this Component thiz, ComponentsIn componentsIn) => componentsIn.GetComponentsFrom<T>(thiz.gameObject);
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static Component GetComponent(this GameObject thiz, Type type, ComponentsIn componentsIn) => componentsIn.GetComponentFrom(thiz, type);
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static Component GetComponent(this Component thiz, Type type, ComponentsIn componentsIn) => componentsIn.GetComponentFrom(thiz.gameObject, type);
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static Component[] GetComponents(this GameObject thiz, Type type, ComponentsIn componentsIn) => componentsIn.GetComponentsFrom(thiz, type);
        /// Extensions for fetching components at, up, and down the hierarchy.
        public static Component[] GetComponents(this Component thiz, Type type, ComponentsIn componentsIn) => componentsIn.GetComponentsFrom(thiz.gameObject, type);

        /// Convenience: Treat an enumerable of game object as an enumerable of its component.
        public static IEnumerable<Component> GetComponent(this IEnumerable<GameObject> thiz, Type type, ComponentsIn componentsIn = default)
        {
            foreach (GameObject gameObject in thiz)
            {
                Component component = componentsIn.GetComponentFrom(gameObject, type);
                if (component != null) yield return component;
            }
        }
        /// Convenience: Treat an enumerable of game object as an enumerable of its component.
        public static IEnumerable<T> GetComponent<T>(this IEnumerable<GameObject> thiz, ComponentsIn componentsIn = default)
        {
            foreach (GameObject gameObject in thiz)
            {
                T t = componentsIn.GetComponentFrom<T>(gameObject);
                if (t != null) yield return t;
            }
        }
        /// Convenience: Treat an enumerable of game object as an enumerable of its component.
        public static IEnumerable<Component[]> GetComponents(this IEnumerable<GameObject> thiz, Type type, ComponentsIn componentsIn = default)
        {
            foreach (GameObject gameObject in thiz)
            {
                Component[] components = componentsIn.GetComponentsFrom(gameObject, type);
                if (components != null && components.Length > 0) yield return components;
            }
        }
        /// Convenience: Treat an enumerable of game object as an enumerable of its component.
        public static IEnumerable<T[]> GetComponents<T>(this IEnumerable<GameObject> thiz, ComponentsIn componentsIn = default)
        {
            foreach (GameObject gameObject in thiz)
            {
                T[] t = componentsIn.GetComponentsFrom<T>(gameObject);
                if (t != null && t.Length > 0) yield return t;
            }
        }

        /// Convenience: Treat an enumerable of components as an enumerable of its gameobject.
        public static IEnumerable<GameObject> GetGameObject(this IEnumerable<Component> thiz)
        {
            foreach (Component component in thiz) yield return component.gameObject;
        }
        /// Convenience: Treat an enumerable of component A as an enumerable of component B.
        public static IEnumerable<Component> GetComponent(this IEnumerable<Component> thiz, Type type, ComponentsIn componentsIn = default)
        => thiz.GetGameObject().GetComponent(type, componentsIn);
        /// Convenience: Treat an enumerable of component A as an enumerable of component B.
        public static IEnumerable<T> GetComponent<T>(this IEnumerable<Component> thiz, ComponentsIn componentsIn = default)
        => thiz.GetGameObject().GetComponent<T>(componentsIn);
        /// Convenience: Treat an enumerable of component A as an enumerable of component B.
        public static IEnumerable<Component[]> GetComponents(this IEnumerable<Component> thiz, Type type, ComponentsIn componentsIn = default)
        => thiz.GetGameObject().GetComponents(type, componentsIn);
        /// Convenience: Treat an enumerable of component A as an enumerable of component B.
        public static IEnumerable<T[]> GetComponents<T>(this IEnumerable<Component> thiz, ComponentsIn componentsIn = default)
        => thiz.GetGameObject().GetComponents<T>(componentsIn);

        public static bool TryGetValue<T>(this IReadOnlyDictionary<T, GameObject> thiz, T key, out Component value, Type type, ComponentsIn componentsIn = default)
        {
            if (!thiz.TryGetValue(key, out GameObject gameObject))
            {
                value = default;
                return false;
            }
            value = gameObject.GetComponent(type, componentsIn);
            return value != null;
        }
        public static bool TryGetValue<T, TComponent>(this IReadOnlyDictionary<T, GameObject> thiz, T key, out TComponent value, ComponentsIn componentsIn = default)
        where TComponent : Component
        {
            if (!thiz.TryGetValue(key, out GameObject gameObject))
            {
                value = default;
                return false;
            }
            value = gameObject.GetComponent<TComponent>(componentsIn);
            return value != null;
        }
        public static bool TryGetValue<T, TComponent1, TComponent2>(this IReadOnlyDictionary<T, TComponent1> thiz, T key, out TComponent2 value, ComponentsIn componentsIn = default)
        where TComponent1 : Component
        where TComponent2 : Component
        {
            if (!thiz.TryGetValue(key, out TComponent1 gameObject))
            {
                value = default;
                return false;
            }
            value = gameObject.GetComponent<TComponent2>(componentsIn);
            return value != null;
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
