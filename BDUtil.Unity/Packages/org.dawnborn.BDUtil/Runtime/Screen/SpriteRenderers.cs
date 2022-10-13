using System;
using BDUtil.Math;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class SpriteRenderers
    {
        [Serializable]
        public struct Snapshot : Snapshots.ISnapshot<Snapshot>
        {
            public OrNil<Sprite> Sprite;
            public HSVA Color;
            public OrNil<bool> FlipX;
            public OrNil<bool> FlipY;

            public Snapshot Lerp(in Snapshot braw, float t)
            {
                Snapshot b = this;
                b.Override(braw);
                b.Sprite = t < .5f ? Sprite : b.Sprite;
                b.Color = HSVA.Lerp(Color, b.Color, t);
                b.FlipX = t < .5f ? FlipX : b.FlipX;
                b.FlipY = t < .5f ? FlipY : b.FlipY;
                return b;
            }
            // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
            public void Override(in Snapshot overrides)
            {
                Sprite = overrides.Sprite.HasValue ? overrides.Sprite.Value : Sprite;
                Color.Override(overrides.Color);
                FlipX = overrides.FlipX.HasValue ? overrides.FlipX : FlipX;
                FlipY = overrides.FlipY.HasValue ? overrides.FlipY : FlipY;
            }

            public override string ToString() => $"Snapshot(Sprite={Sprite},Color={Color},FlipX={FlipX.GetNullable()},FlipY={FlipY.GetNullable()})";
        }
        [Serializable]
        public struct Target : Snapshots.ITarget<Snapshot>
        {
            public static readonly Target NaN = new()
            {
                TargetColor = new(Vectors.NaNH, Vectors.NaNH),
                FuzzFlipX = float.NaN,
                FuzzFlipY = float.NaN
            };
            [Tooltip("Default: Don't change; if non-nil yes-null, clear sprite.")]
            public OrNil<Sprite> TargetSprite;
            [Tooltip("NaN-based terms are taken from the original; others are set.")]
            public Randoms.Fuzzy<HSVA> TargetColor;
            [Tooltip("Odds of setting flipX true (NaN leave alone)")]
            [Range(0, 1)] public float FuzzFlipX;
            [Tooltip("Odds of setting flipY true (NaN leave alone)")]
            [Range(0, 1)] public float FuzzFlipY;
            public Snapshot GetTarget(Snapshots.IFuzzControls fuzzControls, in Snapshot @base)
            {
                Snapshot snapshot = @base;
                if (TargetSprite.HasValue) snapshot.Sprite = TargetSprite;
                HSVA targetColor = snapshot.Color.Overridden(TargetColor.Pivot);
                targetColor.s *= fuzzControls.Power;
                targetColor.v *= fuzzControls.Power;
                if (targetColor.a != 1f) targetColor.a *= fuzzControls.Power;
                snapshot.Color = fuzzControls.Random.Fuzzed(targetColor, TargetColor.Fuzz);
                if (float.IsFinite(FuzzFlipX)) snapshot.FlipX = fuzzControls.Random.RandomTrue(FuzzFlipX);
                if (float.IsFinite(FuzzFlipY)) snapshot.FlipY = fuzzControls.Random.RandomTrue(FuzzFlipY);
                return snapshot;
            }
        }
        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Snapshot GetLocalSnapshot(this SpriteRenderer thiz)
        => new()
        {
            Sprite = thiz.sprite,
            Color = thiz.color,
            FlipX = thiz.flipX,
            FlipY = thiz.flipY,
        };

        /// Consider using GetLocalSnapshot to figure out which fields you _don't want to set_ first...
        public static void SetFromLocalSnapshot(this SpriteRenderer thiz, Snapshot target)
        {
            if (target.Sprite.HasValue) thiz.sprite = target.Sprite.Value;
            thiz.color = ((HSVA)thiz.color).Overridden(target.Color);
            if (target.FlipX.HasValue) thiz.flipX = target.FlipX.Value;
            if (target.FlipY.HasValue) thiz.flipY = target.FlipY.Value;
        }
        public static void AddFromLocalSnapshot(this SpriteRenderer thiz, Snapshot add)
        {
            if (add.Sprite.HasValue) thiz.sprite = add.Sprite.Value;
            thiz.color = (HSVA)thiz.color + add.Color;
            if (add.FlipX.HasValue) thiz.flipX = add.FlipX.Value;
            if (add.FlipY.HasValue) thiz.flipY = add.FlipY.Value;
        }
    }
}
