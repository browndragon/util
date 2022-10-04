using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Sprite")]
    public class SpriteLibrary : Library<Sprite, SpriteLibrary.SpriteParams>
    {
        protected override bool IsEntryForObject(in SpriteParams entry, Sprite obj)
        => entry.Target.Sprite == obj;

        protected override Entry NewEntry(Entry template, Sprite fromObj)
        {
            SpriteParams @params = template.Data;
            SpriteRenderers.Snapshot snapshot = @params.Target;
            snapshot.Sprite = fromObj;
            @params.Target = snapshot;
            @params.Mask |= SpriteRenderers.Masks.Sprite;
            template.Data = @params;
            return template;
        }
        [Serializable]
        public struct Fuzz
        {
            public HSVA FuzzColor;
            // 0 setfalse, 1 settrue, anywhere in between odds.
            [Range(0, 1)] public float FuzzFlipX;
            // 0 setfalse, 1 settrue, anywhere in between odds.
            [Range(0, 1)] public float FuzzFlipY;

            public SpriteRenderers.Snapshot Apply(Player player, SpriteRenderers.Snapshot target)
            {
                HSVA targetColor = target.Color;

                targetColor.s *= player.Power;
                targetColor.v *= player.Power;
                if (targetColor.a != 1f) targetColor.a *= player.Power;

                FuzzColor *= player.Chaos;
                target.Color = targetColor + Math.Fuzz.HSVA(FuzzColor);

                target.FlipX = UnityEngine.Random.value > .5f;
                target.FlipY = UnityEngine.Random.value > .5f;

                return target;
            }
        }

        [Serializable]
        public struct SpriteParams : Player.IPlayable
        {
            public Extent FuzzDuration;
            public SpriteRenderers.Masks Mask;
            public SpriteRenderers.Snapshot Target;
            public Easings.Enum Ease;
            public Fuzz Fuzz;

            public float PlayOn(Player player)
            {
                float duration = FuzzDuration.ScaledBy(player.Chaos).RandomPoint() / player.Speed;
                SpriteRenderer source = player.GetComponent<SpriteRenderer>().OrThrow();
                player.StartCoroutine(new Timer(duration)
                    .Let(out var mask, Mask)
                    .Let(out var ease, Ease)
                    .Let(out var start, source.GetLocalSnapshot())
                    .Let(out var target, Fuzz.Apply(player, Target))
                    .Foreach(t =>
                    {
                        SpriteRenderers.Snapshot slice = source.GetLocalSnapshot(mask, SpriteRenderers.Snapshot.Lerp(start, target, ease.Invoke(t)));
                        source.SetFromLocalSnapshot(slice);
                    })
                );
                return duration;
            }
        }
    }
}
