using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Library
{
    [Tooltip("A generic source of multiple assets (sprites, audio, treasure??).")]
    public abstract class Library : ScriptableObject
    {
        protected internal static readonly string[] EmptyTag = { "" };
        protected internal static float DefaultSafe(float v, float min = 0f) => v == default ? 1f : v < min ? min : v;
        protected readonly Dictionary<string, float> TagOdds = new();
        protected readonly Dictionary<int, string> Hashes = new();

        public enum HashSource
        {
            Short = default,
            FullPath,
            Tag,
            None,
        }
        public HashSource HashSource_;

        public interface IEntry
        {
            IEnumerable<string> Tags { get; }
            float Odds { get; }
            object Data { get; }
        }
        public interface ICategory : IEnumerable
        {
            bool IsValid { get; }
            float Odds { get; }
            int Count { get; }
            object this[int i] { get; }
            IReadOnlyList<IEntry> Entries { get; }
            int GetRandom();
        }

        public abstract ICategory GetICategory(string tag);
        public string GetTagStringFromHash(int hash)
        => Hashes.TryGetValue(hash, out string tag) ? tag : null;
        public string GetTagStringFromHash(AnimatorStateInfo stateInfo, bool isExit)
        {
            if (HashSource_ == HashSource.None) return null;
            int hash = (isExit ? -1 : +1) * (HashSource_ switch
            {
                HashSource.FullPath => stateInfo.fullPathHash,
                HashSource.Short => stateInfo.shortNameHash,
                HashSource.Tag => stateInfo.tagHash,
                _ => throw new NotImplementedException($"Can't get {HashSource_} hash from {stateInfo}"),
            });
            string tag = GetTagStringFromHash(hash);
            Debug.Log($"Got tag {hash}->{tag}");
            return tag;
        }
    }
    [Tooltip("A generic source of multiple assets (sprites, audio, treasure??).")]
    public class Library<T> : Library
    {
        public Entry[] Entries;
        readonly Dictionary<string, List<Entry>> TagEntries = new();

        [Serializable]
        public struct Entry : IEntry
        {
            [Tooltip("These can be animator states; if so, `name` matches state enter and continue; `-name` matches state exit. The hash encoding matches; exits are the negative of the statehash.")]
            [SerializeField] string[] tags;
            public IEnumerable<string> Tags => tags.IsEmpty() ? EmptyTag : tags;
            [SerializeField] float odds;
            public float Odds => DefaultSafe(odds);
            public T Data;
            object IEntry.Data => Data;
        }
        public readonly struct Category : ICategory
        {
            public bool IsValid => Entries != null;
            public readonly float Odds;
            float ICategory.Odds => Odds;
            public readonly IReadOnlyList<Entry> Entries;
            IReadOnlyList<IEntry> ICategory.Entries => Entries.Upcast(default(IEntry));
            public Category(float odds, IReadOnlyList<Entry> entries)
            {
                Odds = odds;
                Entries = entries;
            }
            public int GetRandom()
            {
                if (Entries == null) return -1;
                float chance = UnityEngine.Random.Range(0, Odds);
                for (int i = 0; i < Entries.Count; ++i)
                {
                    if ((chance -= Entries[i].Odds) <= 0f) return i;
                }
                return -1;
            }
            public int Count => Entries.Count;
            public T this[int i] => Entries[i].Data;
            object ICategory.this[int i] => this[i];
            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i) yield return this[i];
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        void OnEnable() => Recalculate();
        void OnValidate() => Recalculate();

        public Category GetCategory(string tag)
        => new(TagOdds.GetValueOrDefault(tag), TagEntries.GetValueOrDefault(tag));
        public override ICategory GetICategory(string tag)
        {
            Category category = GetCategory(tag);
            if (!category.IsValid) return null;
            return category;
        }
        void Recalculate()
        {
            TagOdds.Clear();
            TagEntries.Clear();
            Hashes.Clear();
            if (Entries == null) return;
            foreach (Entry entry in Entries)
            {
                foreach (string tag in entry.Tags)
                {
                    TagOdds[tag] = TagOdds.GetValueOrDefault(tag) + entry.Odds;
                    TagEntries.Add(tag, entry);
                }
            }
            if (HashSource_ == HashSource.None) return;
            foreach (string tag in TagOdds.Keys)
            {
                if (tag.StartsWith("-"))
                {
                    Hashes.Add(-Animator.StringToHash(tag[1..]), tag);
                    continue;
                }
                Hashes.Add(Animator.StringToHash(tag), tag);
            }
        }
    }
}