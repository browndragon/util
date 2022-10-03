using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Transform")]
    public class TransformLibrary : Library<TransformLibrary.Local, TransformLibrary.Easing>
    {
        [Serializable]
        public class Local
        {
            public Vector3 Position;
            public Vector3 EulerAngles;
            public Vector3 Scale;
        }
        [Serializable]
        public class Easing : Player.IPlayable<Local>
        {
            public Extent FuzzDuration = new(.5f, 1f);
            [Serializable]
            public struct FuzzVector
            {
                public Vector3 Fuzz;
                public Easings.Enum Ease;
                public static implicit operator FuzzVector(Vector3 vector)
                => new() { Fuzz = vector };
            }
            public FuzzVector Position = new Vector3(.1f, .1f, 0f);
            public FuzzVector EulerAngles = Vector3.zero;
            public FuzzVector Scale = .1f * Vector3.one;

            public float PlayOn(Player player, Local rawTarget)
            {
                float duration = FuzzDuration.RandomPoint();
                player.StartCoroutine(new Timer(duration)
                    .Let(out var startPos, player.transform.localPosition)
                    .Let(out var startRot, player.transform.eulerAngles)
                    .Let(out var startScale, player.transform.localScale)
                    .Let(out var targetPos, startPos + Fuzz.Vector3(Position.Fuzz))
                    .Let(out var targetRot, startRot + Fuzz.Vector3(EulerAngles.Fuzz))
                    .Let(out var targetScale, startScale + Fuzz.Vector3(Scale.Fuzz))
                    .Foreach(t =>
                    {
                        player.transform.localPosition = Vector3.Lerp(startPos, targetPos, Position.Ease.Invoke(t));
                        player.transform.eulerAngles = Vector3.Lerp(startRot, targetRot, EulerAngles.Ease.Invoke(t));
                        player.transform.localPosition = Vector3.Lerp(startScale, targetScale, Scale.Ease.Invoke(t));
                    }));
                return duration;
            }
        }
    }
}
