using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Transform")]
    public class TransformLibrary : PlayerLibrary<TransformLibrary.Transform>
    {
        [Serializable]
        public class Transform : Player.IPlayable
        {
            public Extent FuzzDuration = new(.125f, .5f);
            public Easings.Enum Ease;
            public Transforms.Masks Mask;
            public Transforms.Local Local;
            public Transforms.Local Fuzz = new()
            {
                Position = .1f * Vector3.one,
                EulerAngles = Vector3.zero,
                Scale = .1f * Vector3.one,
            };

            public float PlayOn(Player player)
            {
                float duration = Math.Fuzz.RandomPoint(FuzzDuration.ScaledBy(player.Chaos)) / player.Speed;
                player.StartCoroutine(new Timer(duration)
                    .Let(out var start, player.transform.GetLocalSnapshot())
                    .Let(out var target, new Transforms.Local()
                    {
                        Position = start.Position + Math.Fuzz.Vector3(player.Chaos * Fuzz.Position),
                        EulerAngles = start.EulerAngles + Math.Fuzz.Vector3(player.Chaos * Fuzz.EulerAngles),
                        // This isn't 0-centered, so to avoid WILD swings, we have to pick a random number that _is_ zero-centered...
                        Scale = start.Scale + Math.Fuzz.Vector3(player.Chaos * (Fuzz.Scale - Vector3.one))
                    })
                    .Foreach(t =>
                    {
                        var eased = Ease.Invoke(t);
                        Transforms.Local lerped = Transforms.Local.Lerp(start, target, eased);
                        lerped = player.transform.GetLocalSnapshot(Mask, lerped);
                        player.transform.SetFromLocalSnapshot(lerped);
                    }));
                return duration;
            }
        }
    }
}
