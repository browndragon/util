using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class AudioSources
    {
        [Serializable]
        public struct Snapshot : Snapshots.ISnapshot<Snapshot>
        {
            public OrNil<AudioClip> AudioClip;
            public float Volume;
            public float Pitch;
            public OrNil<bool> Loop;
            public Snapshot Lerp(in Snapshot braw, float t)
            {
                Snapshot b = this;
                b.Override(braw);
                b.AudioClip = t < .5f ? AudioClip : b.AudioClip;
                b.Volume = Mathf.Lerp(Volume, b.Volume, t);
                b.Pitch = Mathf.Lerp(Pitch, b.Pitch, t);
                b.Loop = t < .5f ? Loop : b.Loop;
                return b;
            }

            // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
            public void Override(in Snapshot overrides)
            {
                AudioClip = overrides.AudioClip.HasValue ? overrides.AudioClip : AudioClip;
                Volume = float.IsFinite(overrides.Volume) ? overrides.Volume : Volume;
                Pitch = float.IsFinite(overrides.Pitch) ? overrides.Pitch : Pitch;
                Loop = overrides.Loop.HasValue ? overrides.Loop : Loop;
            }

            public override string ToString() => $"AudioSnapshot({AudioClip},vol={Volume},pitch={Pitch})";
        }
        [Serializable]
        public struct Target : Snapshots.ITarget<Snapshot>
        {
            public OrNil<AudioClip> AudioClip;
            public Randoms.Fuzzy<float> Volume;
            public Randoms.Fuzzy<float> Pitch;
            [Range(0, 1)] public float Loop;

            public Snapshot GetTarget(Snapshots.IFuzzControls controls, in Snapshot start)
            {
                Snapshot @return = start;
                @return.AudioClip = AudioClip;
                @return.Volume = controls.Random.Fuzzed(Volume.Pivot * controls.Power, Volume.Fuzz, start.Volume);
                @return.Pitch = controls.Random.Fuzzed(Pitch.Pivot * controls.Speed, Pitch.Fuzz, start.Pitch);
                @return.Loop = float.IsFinite(Loop) ? new(controls.Random.RandomTrue(Loop)) : new();
                return @return;
            }
        }
        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Snapshot GetLocalSnapshot(this AudioSource thiz)
        => new()
        {
            AudioClip = thiz.clip,
            Volume = thiz.volume,
            Pitch = thiz.pitch,
            Loop = thiz.loop,
        };
        /// Consider using GetLocalSnapshot to figure out which fields you _don't want to set_ first...
        public static void SetFromLocalSnapshot(this AudioSource thiz, Snapshot target)
        {
            if (float.IsFinite(target.Pitch)) thiz.pitch = target.Pitch;
            if (float.IsFinite(target.Volume)) thiz.volume = target.Volume;
            if (target.Loop.HasValue) thiz.loop = target.Loop.Value;
            if (target.AudioClip.HasValue) thiz.clip = target.AudioClip.Value;
        }
    }
}
