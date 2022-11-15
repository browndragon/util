using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class SpriteRenderers
    {
        [Serializable]
        public struct Snapshot : ISnapshot<SpriteRenderer>
        {
            public Sprite Sprite;
            public Color Color;
            public bool FlipX;
            public bool FlipY;

            public Snapshot(Sprite sprite, Color color, bool flipx, bool flipy)
            {
                Sprite = sprite;
                Color = color;
                FlipX = flipx;
                FlipY = flipy;
            }
            public Snapshot(SpriteRenderer renderer) : this(renderer.sprite, renderer.color, renderer.flipX, renderer.flipY) { }

            public void ReadFrom(SpriteRenderer player) => this = new(player);

            public void ReadFrom(GameObject player) => ReadFrom(player.GetComponent<SpriteRenderer>());

            public void ApplyTo(SpriteRenderer player)
            {
                player.sprite = Sprite;
                player.color = Color;
                player.flipX = FlipX;
                player.flipY = FlipY;
            }

            public void ApplyTo(GameObject player) => ApplyTo(player.GetComponent<SpriteRenderer>());

            public override string ToString() => $"Snapshot(Sprite={Sprite},Color={Color},FlipX={FlipX},FlipY={FlipY})";
        }
        [Serializable]
        public struct Target
        {
            public Sprite Sprite;
            [Tooltip("NaN-based terms are taken from the original; others are set.")]
            public HSVA.Fuzzed Color;
            [Tooltip("Odds of setting flipX true (NaN leave alone)")]
            [Range(0, 1)] public float FlipX;
            [Tooltip("Odds of setting flipY true (NaN leave alone)")]
            [Range(0, 1)] public float FlipY;
        }
        public static Snapshot Range(this Randoms.IRandom thiz, Target target) => new(
            target.Sprite,
            thiz.Range(target.Color),
            thiz.Odds(target.FlipX),
            thiz.Odds(target.FlipY)
        );
    }
}
