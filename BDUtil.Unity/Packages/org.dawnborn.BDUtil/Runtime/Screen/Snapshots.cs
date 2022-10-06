using System;
using BDUtil.Math;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class Snapshots
    {
        public interface ISnapshot { }
        public interface ISnapshot<TSnapshot, TMask> : ISnapshot
        where TSnapshot : ISnapshot<TSnapshot, TMask>
        where TMask : Enum
        {
            TSnapshot Lerp(in TSnapshot other, float t);
            void Override(TMask overrideFields, in TSnapshot overrides);
        }
        public interface IFuzzControls
        {
            public Randoms.UnitRandom Random { get; }
            public float Power { get; }
            public float Speed { get; }
        }
        public interface IFuzz { }
        public interface IFuzz<TSnapshot, TMask> : IFuzz
        where TSnapshot : ISnapshot<TSnapshot, TMask>
        where TMask : Enum
        {
            void Apply(IFuzzControls controls, TMask fuzzFields, in TSnapshot start, ref TSnapshot target);
        }

        [Serializable]
        public struct FuzzTarget<TSnapshot, TMask, TFuzz>
        where TSnapshot : ISnapshot<TSnapshot, TMask>
        where TMask : Enum
        where TFuzz : IFuzz<TSnapshot, TMask>
        {
            public TMask PivotOverrides;
            public TSnapshot Pivot;
            [Tooltip("All `PivotOverrides` can be fuzzed (set the fuzz terms to 0 to avoid); these are also fuzzed & tweened.")]
            [SerializeField] internal TMask alsoOverride;
            public TMask TargetOverrides => Enums<TMask>.FromValue(
                Enums<TMask>.GetValue(PivotOverrides)
                | Enums<TMask>.GetValue(alsoOverride)
            );
            public TFuzz Fuzz;
            public TSnapshot GetTarget(IFuzzControls controls, in TSnapshot start)
            {
                TSnapshot target = start;
                target.Override(PivotOverrides, Pivot);
                Fuzz.Apply(controls, TargetOverrides, start, ref target);
                return target;
            }
        }
        [Serializable]
        public struct Animate<TSnapshot, TMask, TFuzz>
        where TSnapshot : ISnapshot<TSnapshot, TMask>
        where TMask : Enum
        where TFuzz : IFuzz<TSnapshot, TMask>
        {
            public Randoms.Fuzzed<float> Delay;
            public Easings.Enum Easing;
            public FuzzTarget<TSnapshot, TMask, TFuzz> FuzzTarget;
        }
    }
}