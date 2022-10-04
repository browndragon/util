using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class SpriteRenderers
    {
        [Flags]
        public enum Masks
        {
            None = 0,
            Sprite = 1 << 0,
            ColorIsHSV = 1 << 1,
            ColorRH = 1 << 2,
            ColorGS = 1 << 3,
            ColorBV = 1 << 4,
            ColorA = 1 << 5,
            FlipX = 1 << 6,
            FlipY = 1 << 7,
        }
        [Serializable]
        public struct Snapshot
        {
            public Sprite Sprite;
            public Color Color;
            public bool FlipX;
            public bool FlipY;

            public static Snapshot Lerp(Snapshot a, Snapshot b, float t)
            => new()
            {
                Sprite = t <= .5f ? a.Sprite : b.Sprite,
                Color = Color.Lerp(a.Color, b.Color, t),
                FlipX = t <= .5f ? a.FlipX : b.FlipX,
                FlipY = t <= .5f ? a.FlipY : b.FlipY
            };
        }
        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Snapshot GetLocalSnapshot(this SpriteRenderer thiz, Masks masks = default, Snapshot snapshot = default)
        => new()
        {
            Sprite = masks.HasFlag(Masks.Sprite) ? snapshot.Sprite : thiz.sprite,
            Color = UpdateColor(
                thiz.color, snapshot.Color,
                masks.HasFlag(Masks.ColorIsHSV),
                masks.HasFlag(Masks.ColorRH), masks.HasFlag(Masks.ColorGS), masks.HasFlag(Masks.ColorBV), masks.HasFlag(Masks.ColorA)),
            FlipX = masks.HasFlag(Masks.FlipX) ? snapshot.FlipX : thiz.flipX,
            FlipY = masks.HasFlag(Masks.FlipY) ? snapshot.FlipY : thiz.flipY,
        };

        static Color UpdateColor(Color current, Color @override, bool inHSVspace, bool overrideR, bool overrideG, bool overrideB, bool overrideA)
        {
            if (inHSVspace)
            {
                HSVA currHSVA = current;
                HSVA overHSVA = @override;
                HSVA @return = new(
                    overrideR ? overHSVA.h : currHSVA.h,
                    overrideG ? overHSVA.s : currHSVA.s,
                    overrideB ? overHSVA.v : currHSVA.v,
                    overrideA ? overHSVA.a : currHSVA.a
                );
                return @return;
            }
            return new(
                overrideR ? @override.r : current.r,
                overrideG ? @override.g : current.g,
                overrideB ? @override.b : current.b,
                overrideA ? @override.a : current.a
            );
        }
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
