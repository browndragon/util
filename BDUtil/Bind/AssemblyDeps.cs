using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BDUtil.Bind
{
    /// Builds a map of assemblies that co-refer.
    /// This lets us determine the set of assemblies depending on one,
    /// which is particularly nice for figuring out who *might* use an attribute!
    public class AssemblyDeps : IReadOnlyCollection<Assembly>
    {
        readonly Dictionary<string, Assembly> Seed = new();
        public int Count => Seed.Count;

        /// Adds a new element to the seed.
        public void Add(Assembly assembly) => Seed.Add(assembly.FullName, assembly);
        public void Clear() => Seed.Clear();
        public bool Contains(Assembly assembly) => assembly != null && Seed.ContainsKey(assembly.FullName);
        public bool ContainsDep(Assembly assembly)
        {
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                if (Seed.ContainsKey(name.FullName)) return true;
            }
            return false;
        }
        /// Scan assemblies over & over until we're sure we've caught all of the A->B->C chains on any C already in our seed.
        public int AddDepsRecursive(IEnumerable<Assembly> assemblies = null)
        {
            assemblies ??= AppDomain.CurrentDomain.GetAssemblies();
            int had = Count;
            int justHad;
            do
            {
                justHad = Count;
                foreach (Assembly assembly in assemblies)
                {
                    if (Contains(assembly)) continue;
                    if (ContainsDep(assembly)) Add(assembly);
                }
            } while ((Count - justHad) > 0);
            return Count - had;
        }
        public IEnumerator<Assembly> GetEnumerator() => Seed.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
