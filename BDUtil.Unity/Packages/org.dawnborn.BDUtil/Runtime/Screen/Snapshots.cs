using System;
using System.Collections;
using BDUtil.Math;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class Snapshots
    {
        // Should be serializable.
        public interface ISnapshot { }
        public interface ISnapshot<TSnapshot> : ISnapshot
        where TSnapshot : ISnapshot<TSnapshot>
        {
            TSnapshot Lerp(in TSnapshot other, float t);
            /// Applies the non-invalid fields of overrides to me.
            /// Invalid is in the eye of the beholder.
            void Override(in TSnapshot overrides);
        }

        public interface IFuzzControls
        {
            Randoms.UnitRandom Random { get; }
            float Power { get; }
            float Speed { get; }

            Camera camera { get; }
            Transform transform { get; }
            Transforms.Snapshot transformSnapshot { get; }
            SpriteRenderer renderer { get; }
            SpriteRenderers.Snapshot rendererSnapshot { get; }
            AudioSource audio { get; }
            AudioSources.Snapshot audioSnapshot { get; }
            Coroutine StartCoroutine(IEnumerator enumerator);

        }
        public interface ITarget { }
        public interface ITarget<TSnapshot> : ITarget
        where TSnapshot : ISnapshot<TSnapshot>
        {
            TSnapshot GetTarget(IFuzzControls controls, in TSnapshot start);
        }

        [Serializable]
        public struct Animate<TSnapshot, TTarget>
        where TSnapshot : ISnapshot<TSnapshot>
        where TTarget : ITarget<TSnapshot>
        {
            public Randoms.Fuzzy<float> Delay;
            public Easings.Enum Easing;
            public TTarget Target;
        }
    }
}