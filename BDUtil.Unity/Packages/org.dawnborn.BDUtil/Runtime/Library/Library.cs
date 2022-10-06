using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Fluent;
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
        [Tooltip("Multiply randomness factor on generated content")]
        public float Chaos = 1f;
        [Tooltip("Multiply power factor on generated content - how loud, etc")]
        public float Power = 1f;
        [Tooltip("Multiply power factor on generated content - how long, etc")]
        public float Speed = 1f;

        [Tooltip("Animator support: how to make tags & animator states agree on hashes")]
        public enum HashSource
        {
            Short = default,
            FullPath,
            [Tooltip("Hash the state tag, not the state name")]
            Tag,
            [Tooltip("That is, _all_ entries go to '' and all exits to '-'")]
            EntryAndExitOnly,
            None,
        }
        public HashSource HashSource_;

        public interface IEntry
        {
            /// Collections this entry should be placed under. For instance, if you have an AudioLibrary Footsteps,
            /// you might want tags for "onStone", "onWood", "onMud", etc. The default is always `""`.
            /// Additionally, tags are automatically hashed (using HashSource_) so that mechanim state enter events
            /// (which will match hashes with the tag of the same name)
            IEnumerable<string> Tags { get; }
            // How likely it is that this object be the one selected.
            float Odds { get; }
            // The payload, which might include "wrapping" information.
            object Data { get; }
        }
        public interface ICategory
        {
            bool IsValid { get; }
            /// The sum of the odds in Entries.
            float Odds { get; }
            IReadOnlyList<IEntry> Entries { get; }
            /// Gets the index of an entry in Entries weighted by their individual odds.
            int GetRandom();
        }

        public abstract ICategory GetICategory(string tag);
        public string GetTagStringFromHash(int hash)
        => Hashes.TryGetValue(hash, out string tag) ? tag : null;
        public string GetTagStringFromHash(AnimatorStateInfo stateInfo, bool isExit)
        {
            string tag;
            int hash = isExit ? -1 : +1;
            switch (HashSource_)
            {
                case HashSource.None: return null;
                case HashSource.EntryAndExitOnly: return isExit ? "" : "-";
                case HashSource.FullPath:
                    tag = GetTagStringFromHash(hash *= stateInfo.fullPathHash);
                    break;
                case HashSource.Short:
                    tag = GetTagStringFromHash(hash *= stateInfo.shortNameHash);
                    break;
                case HashSource.Tag:
                    tag = GetTagStringFromHash(hash *= stateInfo.tagHash);
                    break;
                default: throw new NotImplementedException($"Can't handle {HashSource_}");
            };
            return tag;
        }
        // Not always supported; play the given entry from this library on the player, returning its duration.
        public abstract float Play(ILibraryPlayer player, IEntry entry);
    }
    [Tooltip("A generic source of multiple assets (sprites, audio, treasure??) with rules to pick between them.")]
    public abstract class Library<TObj, TData> : Library
    {
        [Tooltip("If this is a UnityEngine.Object, you can drag assets here to create entries")]
        [SerializeField] protected TObj[] DragAndDropTargets;
        public List<Entry> Entries = new();
        readonly Dictionary<string, List<Entry>> TagEntries = new();

        protected abstract bool IsEntryForObject(in TData data, TObj obj);
        protected abstract Entry NewEntry(Entry template, TObj fromObj);

        // Not always supported; play the given entry from this library on the player, returning its duration.
        public override float Play(ILibraryPlayer player, IEntry entry) => Play(player, ((Entry)entry).Data);
        protected abstract float Play(ILibraryPlayer player, TData entry);

        [Serializable]
        public struct Entry : IEntry
        {
            [SuppressMessage("IDE", "IDE0044")]
            [Tooltip("These can be animator states; if so, `name` matches state enter and continue; `-name` matches state exit. The hash encoding matches; exits are the negative of the statehash.")]
            [SerializeField] string[] tags;
            public IEnumerable<string> Tags => tags.IsEmpty() ? EmptyTag : tags;
            [SuppressMessage("IDE", "IDE0044")]
            [SerializeField] float odds;
            public float Odds => DefaultSafe(odds);
            public TData Data;
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
        }

        protected void OnEnable() => Recalculate();
        protected void OnValidate()
        {
            if (DragAndDropTargets != null)
            {
                foreach (TObj obj in DragAndDropTargets)
                {
                    if (obj == null) continue;
                    if (HasEntryForObject(obj)) continue;
                    Entry @new = Entries.Count > 0 ? Entries[^1] : default;
                    @new = NewEntry(@new, obj);
                    Entries.Add(@new);
                }
                DragAndDropTargets = null;
            }
            Recalculate();
        }
        protected virtual bool HasEntryForObject(TObj obj)
        {
            foreach (Entry entry in Entries) if (IsEntryForObject(entry.Data, obj)) return true;
            return false;
        }

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
    // Convenience.
    public abstract class Library<TData> : Library<Void, TData>
    {
        protected override bool IsEntryForObject(in TData data, Void obj) => false;
        protected override bool HasEntryForObject(Void obj) => false;
        protected override Entry NewEntry(Entry template, Void fromObj) => template;
    }
}