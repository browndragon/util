using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BDUtil.Bind;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Serialization.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization
{
    public readonly struct TypeKey : Choices.IKey, IEquatable<TypeKey>
    {
        internal static readonly Type TypeUEO = typeof(UnityEngine.Object);
        public readonly Type Type;
        public readonly GUIContent Opt0;
        public readonly tern Serializable;
        public readonly tern Instantiable;  // Not abstract or interface
        public readonly tern UnityTypes;  // T:MonoBehaviour, T:ScriptableObject
        public bool PrintDebug { get; }
        public TypeKey(Type type, GUIContent opt0 = default, bool? serializable = true, bool? instantiable = true, bool? unityTypes = false, bool printDebug = false)
        {
            Type = type;
            Opt0 = opt0 ?? new("<null>");
            Serializable = serializable;
            Instantiable = instantiable;
            UnityTypes = unityTypes;
            PrintDebug = printDebug;
        }
        public TypeKey(Type type, SubtypeAttribute attribute)
        : this(type)
        {
            if (attribute != null)
            {
                Serializable = attribute.Serializable;
                Instantiable = attribute.Instantiable;
                UnityTypes = attribute.Unity;
                PrintDebug = attribute.PrintDebug;
            }
        }
        void AddMatchingObjects(List<Type> objects)
        {
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(this.Type);
            if (PrintDebug) Debug.Log($"Calculating subclass {this} of {types.Count} derived");
            for (int i = 0; i < types.Count; ++i)
            {
                // Filter out monobehaviours; we never want to offer an assign option to them; it crashes on attempt to create.
                if (this.UnityTypes ^ TypeUEO.IsAssignableFrom(types[i]))
                {
                    if (PrintDebug) Debug.Log($"{this.Type}: skipping {types[i]} {(this.UnityTypes ? "!:" : ":")} {TypeUEO}");
                    continue;
                }
                // filter out non-new()-able types, if they insist on new()-able types.
                if (!~this.Instantiable)
                {
                    bool isAbstract = types[i].IsAbstract;
                    bool isInterface = types[i].IsInterface;
                    bool isInstantiable = !(isAbstract || isInterface);
                    if (this.Instantiable ^ isInstantiable)
                    {
                        // Not quite true! This checks whether it's abstract; we don't actually check for a 0arg ctor.
                        // This isn't nuts; structs *don't* have a 0arg ctor.
                        if (PrintDebug) Debug.Log($"{this.Type}: skipping {types[i]} {(this.Instantiable ? "!:" : ":")} new()");
                        continue;
                    }
                }
                if (!~this.Serializable)
                {
                    bool isSerializable = !types[i].GetCustomAttributes<SerializableAttribute>().IsEmpty();
                    if (this.Serializable ^ isSerializable)
                    {
                        if (PrintDebug) Debug.Log($"{this.Type}: skipping {types[i]} {(this.Serializable ? "!<" : "<")} Serializable");
                        continue;
                    }
                }
                objects.Add(types[i]);
            }
        }
        Choices Choices.IKey.EvaluateChoices()
        {
            List<Type> objects = new() { null };
            AddMatchingObjects(objects);

            GUIContent[] labels = new GUIContent[objects.Count];
            labels[0] = new(Opt0);
            StringBuilder builder = new();
            for (int i = 1; i < objects.Count; ++i)
            {
                if (objects[i] == null) throw new NullReferenceException();
                Type sub = objects[i];
                foreach (NamedAttribute name in sub.GetCustomAttributes<NamedAttribute>())
                {
                    if (builder.Length > 0) builder.Append(" aka ");
                    builder.Append(name.GetName(sub));
                }
                if (builder.Length > 0) { labels[i] = new(builder.ToString()); builder.Clear(); }
                else labels[i] = new(sub.Name);
            }
            if (PrintDebug) Debug.Log($"Kept {objects.Count} options");
            return new()
            {
                Objects = objects,
                Labels = labels,
                // Probably we should cache this on the instance,
                // but it was still buggy last time I tried.
                Index = -1,
            };
        }

        public bool Equals(TypeKey other)
        => (Type != null ? Type.Equals(other.Type) : other.Type == null)
        && Serializable == other.Serializable
        && Instantiable == other.Instantiable
        && UnityTypes == other.UnityTypes;
        public bool Equals(Choices.IKey key) => key is TypeKey other && Equals(other);
        public override bool Equals(object obj) => obj is TypeKey other && Equals(other);
        public override int GetHashCode() => Chain.Hash ^ Type ^ Serializable ^ Instantiable ^ UnityTypes;
        // public static implicit operator TypeKey(Type t) => new(t);
        public override string ToString() => $"{Type}: Serial:{Serializable} Instant:{Instantiable} Unity:{UnityTypes}";
    }
}