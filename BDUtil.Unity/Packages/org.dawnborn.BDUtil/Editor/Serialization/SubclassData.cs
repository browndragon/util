using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BDUtil.Bind;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    public class SubclassData
    {
        public readonly Type Base;

        readonly List<Type> subs = new();
        public IReadOnlyList<Type> Subs => subs;
        public string[] SubNames { get; private set; }

        public SubclassData(Type @base) => Base = @base;

        internal void Calculate()
        {
            Type monoBehaviour = typeof(MonoBehaviour);
            subs.Add(null);
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(Base);
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
                    Debug.Log($"Rejecting {types[i]} for {Base} because it isn't serializable");
                    continue;
                }
                subs.Add(types[i]);
            }
            SubNames = new string[subs.Count];
            SubNames[0] = "<null>";
            StringBuilder builder = new();
            for (int i = 1; i < subs.Count; ++i)
            {
                Type sub = subs[i].OrThrow();
                foreach (NamedAttribute name in sub.GetCustomAttributes<NamedAttribute>())
                {
                    if (builder.Length > 0) builder.Append(" aka ");
                    builder.Append(name.GetName(sub));
                }
                if (builder.Length > 0)
                {
                    SubNames[i] = builder.ToString();
                    builder.Clear();
                }
                else
                {
                    SubNames[i] = sub.Name;
                }
            }
        }
        public int Find(Type type)
        {
            if (type == null) return 0;
            for (int i = 1; i < subs.Count; ++i)
            {
                Type at = subs[i];
                if (type == at) return i;
            }
            return -1;
        }
    }
}