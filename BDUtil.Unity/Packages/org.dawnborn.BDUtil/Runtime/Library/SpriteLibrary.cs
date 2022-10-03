using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Sprite")]
    public class SpriteLibrary : Library<Sprite, SpriteLibrary.SpriteParams>
    {
        [Serializable]
        public struct SpriteParams : Player.IPlayable<Sprite>
        {
            public Extent Length;
            public float NormalizedSprite;
            public HSVA BaseColor;
            public HSVA FuzzColor;
            public Easings.Enum EaseColor;

            // 0 setfalse, 1 settrue, anywhere in between odds.
            [Range(0, 1)] public float FlipX;
            // 0 setfalse, 1 settrue, anywhere in between odds.
            [Range(0, 1)] public float FlipY;

            public float PlayOn(Player player, Sprite sprite)
            {
                float length = Length.RandomPoint();
                SpriteRenderer source = player.GetComponent<SpriteRenderer>().OrThrow();
                if (sprite != null) source.sprite = sprite;
                if (!float.IsNaN(FlipX)) source.flipX = UnityEngine.Random.value >= .5f;
                if (!float.IsNaN(FlipY)) source.flipY = UnityEngine.Random.value >= .5f;
                if (EaseColor == Easings.Enum.None)
                {
                    source.color = BaseColor + Fuzz.HSVA(FuzzColor);
                    return length;
                }
                player.StartCoroutine(new Timer(length)
                    .Let(out var ease, EaseColor)
                    .Let(out var start, source.color)
                    .Let(out var target, BaseColor + Fuzz.HSVA(FuzzColor))
                    .Foreach(t => source.color = HSVA.Lerp(start, target, ease.Invoke(t)))
                );
                return length;
            }
        }
    }
}
