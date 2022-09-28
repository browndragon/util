using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    /// The list of choices & their labels, kept carefully in sync externally.
    [Serializable]
    public struct Choices
    {
        public IReadOnlyList<object> Objects;
        public GUIContent[] Labels;
        public int Index;
        public void Deconstruct(out IReadOnlyList<object> objects, out GUIContent[] labels, out int index)
        {
            objects = Objects;
            labels = Labels;
            index = Index;
        }

        public interface IKey : IEquatable<IKey> { Choices EvaluateChoices(); }
        static readonly Dictionary<IKey, Choices> cache = new();
        public static Choices Get(IKey choicesKey)
        => cache.TryGetValue(choicesKey, out Choices choices)
        ? choices
        : Reset(choicesKey);
        public static Choices Reset(IKey choicesKey)
        => cache[choicesKey] = choicesKey.EvaluateChoices();
        public static void ClearAll() => cache.Clear();
    }
}