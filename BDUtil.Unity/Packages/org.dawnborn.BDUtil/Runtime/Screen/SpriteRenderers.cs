using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class SpriteRenderers
    {
        [Flags]
        public enum Overrides
        {
            None = 0,
            ColorsOpaque = ColorRH | ColorGS | ColorBV,
            ColorRH = 1 << 0,
            ColorGS = 1 << 1,
            ColorBV = 1 << 2,
            ColorA = 1 << 3,
            ColorIsHSV = 1 << 4,
            FlipX = 1 << 5,
            FlipY = 1 << 6,
            Sprite = 1 << 7,
        }
        public static Colors.Overrides AsColorOverrides(this Overrides thiz)
        => Enums<Colors.Overrides>.Everything & Enums<Colors.Overrides>.FromValue(Enums<Overrides>.GetValue(thiz));
        public static Overrides AsSpriteOverrides(this Colors.Overrides thiz)
        => Enums<Overrides>.FromValue(Enums<Colors.Overrides>.GetValue(thiz));

        [Serializable]
        public struct Snapshot : Snapshots.ISnapshot<Snapshot, Overrides>
        {
            public Sprite Sprite;
            public Color Color;
            public bool FlipX;
            public bool FlipY;

            public Snapshot Lerp(in Snapshot b, float t)
            => new()
            {
                Sprite = t <= .5f ? Sprite : b.Sprite,
                Color = Color.Lerp(Color, b.Color, t),
                FlipX = t <= .5f ? FlipX : b.FlipX,
                FlipY = t <= .5f ? FlipY : b.FlipY
            };
            // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
            public void Override(Overrides overrideField, in Snapshot overrides)
            {
                Sprite = overrideField.HasFlag(Overrides.Sprite) ? overrides.Sprite : Sprite;
                Color.Override(overrideField.AsColorOverrides(), overrides.Color);
                FlipX = overrideField.HasFlag(Overrides.FlipX) ? overrides.FlipX : FlipX;
                FlipY = overrideField.HasFlag(Overrides.FlipY) ? overrides.FlipY : FlipY;
            }

            public override string ToString() => $"Snapshot(Sprite={Sprite},Color={Color}{(FlipX ? ",FlipX" : "")}{(FlipY ? ",FlipY" : "")})";
        }
        [Serializable]
        public struct Fuzz : Snapshots.IFuzz<Snapshot, Overrides>
        {
            public HSVA FuzzColor;
            // 0 setfalse, 1 settrue, anywhere in between odds.
            [Range(0, 1)] public float FuzzFlipX;
            // 0 setfalse, 1 settrue, anywhere in between odds.
            [Range(0, 1)] public float FuzzFlipY;

            public void Apply(Snapshots.IFuzzControls fuzzControls, Overrides overrideFields, in Snapshot @base, ref Snapshot target)
            {
                HSVA targetColor = target.Color;
                targetColor.s *= fuzzControls.Power;
                targetColor.v *= fuzzControls.Power;
                if (targetColor.a != 1f) targetColor.a *= fuzzControls.Power;
                target.Color.Override(overrideFields.AsColorOverrides(), targetColor + fuzzControls.Random.Range(-FuzzColor, FuzzColor));

                if (overrideFields.HasFlag(Overrides.FlipX)) target.FlipX ^= UnityEngine.Random.value > FuzzFlipX;
                if (overrideFields.HasFlag(Overrides.FlipY)) target.FlipY ^= UnityEngine.Random.value > FuzzFlipY;
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
            thiz.sprite = target.Sprite;
            thiz.color = target.Color;
            thiz.flipX = target.FlipX;
            thiz.flipY = target.FlipY;
        }
    }
}
