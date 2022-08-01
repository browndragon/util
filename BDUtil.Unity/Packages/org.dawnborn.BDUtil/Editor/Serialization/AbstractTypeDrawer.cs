using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BDUtil.Bind;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    /// Selects between types tagged with some restriction.
    /// See SubtypeDrawer and ByRefDrawer for more.
    public abstract class AbstractTypeDrawer : ChoiceDrawer<Type>
    {
        // Typecache of base->[concrete]subclasses \ UnityEngine.Object.
        // TODO: clear?
        static readonly Dictionary<Type, Choices> cache = new();
        // per-instance (?? Reuse safe? Seems to be...) subclass drawer.
        // TODO: clear? Less critical, destroying instance kills it.
        // readonly Dictionary<string, int> index = new();
        internal static Choices GetCachedSubclassData(Type @base)
        {
            if (cache.TryGetValue(@base, out Choices cached)) return cached;
            cached = CalculateSubclassData(@base);
            cache.Add(@base, cached);
            return cached;
        }
        static Choices CalculateSubclassData(Type @base)
        {
            List<Type> objects = new();
            Type monoBehaviour = typeof(MonoBehaviour);
            objects.Add(null);
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(@base);
            for (int i = 0; i < types.Count; ++i)
            {
                // Filter out monobehaviours; we never want to offer an assign option to them; it crashes on attempt to create.
                if (monoBehaviour.IsAssignableFrom(types[i])) continue;

                // Filter out un-serializable types; we didn't want 'em anyway.
                bool isSerializable = false;
                foreach (SerializableAttribute serializable in types[i].GetCustomAttributes<SerializableAttribute>())
                {
                    isSerializable = true;
                    break;
                }
                if (!isSerializable)
                {
                    Debug.Log($"Rejecting {types[i]} for {@base} because it isn't serializable");
                    continue;
                }
                objects.Add(types[i]);
            }
            string[] labels = new string[objects.Count];
            labels[0] = "<null>";
            StringBuilder builder = new();
            for (int i = 1; i < objects.Count; ++i)
            {
                Type sub = objects[i].OrThrow();
                foreach (NamedAttribute name in sub.GetCustomAttributes<NamedAttribute>())
                {
                    if (builder.Length > 0) builder.Append(" aka ");
                    builder.Append(name.GetName(sub));
                }
                if (builder.Length > 0) { labels[i] = builder.ToString(); builder.Clear(); }
                else labels[i] = sub.Name;
            }
            return new()
            {
                Objects = objects,
                Labels = labels,
                // Probably we should cache this on the instance,
                // but it was still buggy last time I tried.
                Index = -1,
            };
        }
    }
}