using System;
using System.Collections.Generic;
using System.Linq;
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
        static readonly Type TypeUEO = typeof(UnityEngine.Object);

        protected readonly struct TypeKey : IEquatable<TypeKey>
        {
            public readonly Type Type;
            public readonly tern Serializable;
            public readonly tern Instantiable;  // Not abstract or interface
            public readonly tern UnityTypes;  // T:MonoBehaviour, T:ScriptableObject
            public TypeKey(Type type, bool? serializable = true, bool? instantiable = true, bool? unityTypes = false)
            {
                Type = type;
                Serializable = serializable;
                Instantiable = instantiable;
                UnityTypes = unityTypes;
            }
            public TypeKey(Type type, SubtypeAttribute attribute)
            : this(type)
            {
                if (attribute != null)
                {
                    Serializable = attribute.Serializable;
                    Instantiable = attribute.Instantiable;
                    UnityTypes = attribute.Unity;
                }
            }

            public bool Equals(TypeKey other)
            => (Type != null ? Type.Equals(other.Type) : other.Type == null)
            && Serializable == other.Serializable
            && Instantiable == other.Instantiable
            && UnityTypes == other.UnityTypes;

            public override bool Equals(object obj) => obj is TypeKey other && Equals(other);
            public override int GetHashCode() => HashCode.Default.And(Type).And(Serializable).And(Instantiable).And(UnityTypes);
            // public static implicit operator TypeKey(Type t) => new(t);
            public override string ToString() => $"{Type}: Serial:{Serializable} Instant:{Instantiable} Unity:{UnityTypes}";
        }
        // Typecache of base->[concrete]subclasses \ UnityEngine.Object.
        // TODO: clear?
        static readonly Dictionary<TypeKey, Choices> cache = new();
        // per-instance (?? Reuse safe? Seems to be...) subclass drawer.
        // TODO: clear? Less critical, destroying instance kills it.
        // readonly Dictionary<string, int> index = new();
        protected static Choices GetCachedSubclassData(TypeKey @base, bool printDebug = false)
        {
            if (cache.TryGetValue(@base, out Choices cached)) return cached;
            cached = CalculateSubclassData(@base, printDebug);
            cache.Add(@base, cached);
            return cached;
        }
        static Choices CalculateSubclassData(TypeKey @base, bool printDebug = false)
        {
            List<Type> objects = new() { null };
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(@base.Type);
            if (printDebug) Debug.Log($"Calculating subclass {@base} of {types.Count} derived");
            for (int i = 0; i < types.Count; ++i)
            {
                // Filter out monobehaviours; we never want to offer an assign option to them; it crashes on attempt to create.
                if (@base.UnityTypes ^ TypeUEO.IsAssignableFrom(types[i]))
                {
                    if (printDebug) Debug.Log($"{@base.Type}: skipping {types[i]} {(@base.UnityTypes ? "!:" : ":")} {TypeUEO}");
                    continue;
                }
                // filter out non-new()-able types, if they insist on new()-able types.
                if (!~@base.Instantiable)
                {
                    if (@base.Instantiable ^ types[i].GetConstructors().Any(c => c.GetParameters().Count() == 0))
                    {
                        if (printDebug) Debug.Log($"{@base.Type}: skipping {types[i]} {(@base.Instantiable ? "!:" : ":")} new()");
                        continue;
                    }
                }
                if (!~@base.Serializable)
                {
                    if (@base.Serializable ^ !types[i].GetCustomAttributes<SerializableAttribute>().IsEmpty())
                    {
                        if (printDebug) Debug.Log($"{@base.Type}: skipping {types[i]} {(@base.Serializable ? "!<" : "<")} Serializable");
                        continue;
                    }
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