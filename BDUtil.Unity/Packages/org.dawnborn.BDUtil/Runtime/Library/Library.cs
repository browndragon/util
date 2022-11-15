using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Library
{
    [Tooltip("A generic source of multiple assets (sprites, audio, treasure??).")]
    public abstract class Library : ScriptableObject
    {
        protected internal static readonly string[] EmptyTag = { "" };
        protected readonly Dictionary<int, string> Hashes = new();

        [Serializable]
        public struct Statistics
        {
            public static readonly Statistics One = new() { odds0 = 0, cost0 = 0 };
            [Tooltip("1+odds0=share of the odds that this is selected")]
            [SerializeField] internal float odds0;
            public float Odds => 1 + odds0;
            [Tooltip("1+cost0=cost if this is selected (arbitrary units). Entries are sorted!")]
            [SerializeField] internal float cost0;
            public float Cost => 1 + cost0;
            public static float SumOddsBelowCost(IEnumerable<Statistics> stats, float maxCost, out int limit)
            {
                float odds = 0f;
                limit = 0;
                foreach (Statistics stat in stats)
                {
                    if (stat.Cost > maxCost) return odds;
                    odds += stat.Odds;
                    limit++;
                }
                return odds;
            }
        }

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
            Statistics Statistics { get; }
            // The payload, which might include "wrapping" information; for instance, an AudioClip together with modified float Volume.
            object Data { get; }
        }

        public readonly struct Category
        {
            public readonly float MaxCost;
            public readonly float TotalOdds;
            public readonly IReadOnlyList<IEntry> Entries;
            public bool IsValid => Entries != null;
            public Category(float maxCost, float totalOdds, IReadOnlyList<IEntry> entries)
            {
                MaxCost = maxCost;
                TotalOdds = totalOdds;
                Entries = entries;
            }
            public IEnumerable<Statistics> Statistics => Entries.Select(e => e.Statistics);
            public IEnumerable<float> Odds => Entries.Select(e => e.Statistics.Odds);
        }

        [Serializable]
        public struct EntryChooser
        {
            public static readonly EntryChooser Default = new() { Category = "", Index = -1 };

            [Tooltip("The library will be queried for this tag ('' is the default!)")]
            public string Category;
            public int Index;
            /// An amount of cost we're tracking for our choices.
            public float Cost;
            public enum Strategies
            {
                [Tooltip("Pick an item from the current category by odds")]
                Random = default,
                [Tooltip("Play the 'next' value in the current category")]
                RoundRobin,
                [Tooltip("Plays the given Category/Index value; you'll need to externally adjust.")]
                External,
                [Tooltip("Random ignoring the odds, with each index value having equal odds")]
                IndexRandom,
                [Tooltip("Random from those which we can currently afford, maintaining that value")]
                CostRandom,
            }
            public Strategies Strategy;
            public IEntry ChooseNext(Library library)
            {
                Category category = library.GetCategory(Category);
                Index = PickIndex(category);
                if (Index.IsInRange(0, category.Entries.Count))
                {
                    Cost -= category.Entries[Index].Statistics.Cost;
                    return category.Entries[Index];
                }
                return default;
            }
            public int PickIndex(Category category)
            => Strategy switch
            {
                Strategies.External => Index.PosMod(category.Entries.Count),
                Strategies.IndexRandom => Randoms.main.Range(0, category.Entries.Count),
                Strategies.Random => GetRandom(category),
                Strategies.CostRandom => GetRandom(category, Cost),
                Strategies.RoundRobin => (Index + 1).PosMod(category.Entries.Count),
                _ => throw Strategy.BadValue(),
            };

            public int GetRandom(Category category, float maxCost = float.PositiveInfinity)
            {
                if (category.Entries == null || category.Entries.Count < 0) return -1;
                if (maxCost >= category.MaxCost) return Randoms.main.Index(category.TotalOdds, category.Odds);
                float odds = Statistics.SumOddsBelowCost(category.Statistics, maxCost, out int limit);
                if (limit <= 0) return -1;
                return Randoms.main.Index(odds, category.Odds);
            }
        }
        public abstract Category GetCategory(string tag);
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
    }
    public abstract class Library<TData> : Library
    {
        public List<Entry> Entries = new();
        readonly Dictionary<string, List<Entry>> EntryByTag = new();
        readonly Dictionary<string, (float maxCost, float totalOdds)> StatisticsByTag = new();

        [Serializable]
        public struct Entry : IEntry
        {
            [SuppressMessage("IDE", "IDE0044")]
            [Tooltip("These can be animator states; if so, `name` matches state enter and continue; `-name` matches state exit. The hash encoding matches; exits are the negative of the statehash.")]
            [SerializeField] string[] tags;
            public IEnumerable<string> Tags => tags.IsEmpty() ? EmptyTag : tags;
            [SerializeField] internal Statistics Statistics;
            Statistics IEntry.Statistics => Statistics;
            public TData Data;
            object IEntry.Data => Data;
        }
        protected void OnEnable() => Recalculate();
        public override Category GetCategory(string tag)
        {
            (float maxCost, float totalOdds) = StatisticsByTag.GetValueOrDefault(tag);
            return new(maxCost, totalOdds, EntryByTag.GetValueOrDefault(tag).Upcast(default(IEntry)));
        }
        protected void Recalculate()
        {
            StatisticsByTag.Clear();
            EntryByTag.Clear();
            Hashes.Clear();
            if (Entries == null) return;
            foreach (Entry entry in Entries)
            {
                foreach (string tag in entry.Tags) EntryByTag.Add(tag, entry);
            }
            if (HashSource_ == HashSource.None) return;
            foreach (string tag in EntryByTag.Keys)
            {
                // Sort & calculate total odds.
                List<Entry> entries = EntryByTag[tag];
                entries.Sort((x, y) => x.Statistics.Cost.CompareTo(y.Statistics.Cost));
                float odds = Statistics.SumOddsBelowCost(entries.Select(e => e.Statistics), float.PositiveInfinity, out int limit);
                float maxCost = entries[^1].Statistics.Cost;
                StatisticsByTag[tag] = (maxCost, odds);
                if (tag.StartsWith("-"))
                {
                    Hashes.Add(-Animator.StringToHash(tag[1..]), tag);
                    continue;
                }
                Hashes.Add(Animator.StringToHash(tag), tag);
            }
        }
    }
    [Tooltip("A library whose data is some configuration + a unity object (like audioclip + sound).")]
    public abstract class Library<TData, TObj> : Library<TData>
    where TObj : UnityEngine.Object
    {
        [Tooltip("If this is a UnityEngine.Object, you can drag assets here to create entries")]
        [SerializeField] protected TObj[] DragAndDropTargets;
        protected abstract bool IsEntryForObject(in TData data, TObj obj);
        protected virtual Entry NewEntry(Entry template, TObj fromObj)
        {
            template.Data = NewEntry(template.Data, fromObj);
            return template;
        }
        protected abstract TData NewEntry(TData template, TObj fromObj);
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
    }
}