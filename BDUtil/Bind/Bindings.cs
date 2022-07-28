using System;
using System.Collections.Generic;
using System.Reflection;

namespace BDUtil.Bind
{
    /// Utility cache of instances tagged with some BindAttribute.
    public class Bindings<T> where T : BindAttribute
    {
        // The rank + type of already encountered types in the cache.
        public struct Fitness : IComparable<Fitness>
        {
            public int Rank;
            public Type Type;
            public int CompareTo(Fitness other) => Rank.CompareTo(other.Rank);
        }
        readonly Dictionary<object, List<Fitness>> cache = new();

        public IEnumerable<Fitness> GetFitnesses(object key)
        => cache.TryGetValue(key, out var fs) ? fs : Array.Empty<Fitness>();
        public Type GetBestType(object key)
        {
            foreach (Fitness fitness in GetFitnesses(key)) return fitness.Type;
            return null;
        }
        // Note: uncached!
        public object GetBestInstance(object key)
        {
            Type best = GetBestType(key);
            if (best == null) return null;
            return Activator.CreateInstance(best);
        }

        public void ResetCacheTypes(IEnumerable<Type> types)
        {
            cache.Clear();
            AddCacheTypes(types);
        }
        public void AddCacheTypes(IEnumerable<Type> types)
        {
            foreach (Type type in types) AddCacheType(type);
        }
        void InsertFitness(object key, int rank, Type type)
        {
            if (!cache.TryGetValue(key, out var fitnesses)) cache[key] = fitnesses = new();
            Fitness fitness = new() { Rank = rank, Type = type };
            int index = fitnesses.BinarySearch(fitness);
            fitnesses.Insert(index >= 0 ? index : ~index, fitness);
        }
        public void AddCacheType(Type type)
        {
            foreach (T attribute in type.GetCustomAttributes<T>())
            {
                foreach (object key in attribute.GetKeys(type))
                {
                    InsertFitness(key, attribute.Rank, type);
                }
            }
        }

        public static readonly Bindings<T> Default;
        static Bindings()
        {
            Default = new();

            AssemblyDeps deps = new();
            deps.Add(typeof(T).Assembly);
            deps.AddDepsRecursive();

            foreach (Assembly assembly in deps)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    Default.AddCacheType(type);
                }
            }
        }
    }
    public static class Bindingses
    {
        public static T GetBestInstance<T>(this Bindings<ImplAttribute> thiz)
        => (T)thiz.GetBestInstance(typeof(T));
    }
}
