using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class AudioSources
    {
        [Flags]
        public enum Overrides
        {
            None = 0,
            AudioClip = 1 << 0,
            Volume = 1 << 1,
            Pitch = 1 << 2,
        }
        [Serializable]
        public struct Snapshot : Snapshots.ISnapshot<Snapshot, Overrides>
        {
            public AudioClip AudioClip;
            public float Volume;
            public float Pitch;
            public Snapshot Lerp(in Snapshot b, float t) => new()
            {
                AudioClip = t < .5f ? AudioClip : b.AudioClip,
                Volume = Mathf.Lerp(Volume, b.Volume, t),
                Pitch = Mathf.Lerp(Pitch, b.Pitch, t),
            };

            // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
            public void Override(Overrides overrideField, in Snapshot overrides)
            {
                AudioClip = overrideField.HasFlag(Overrides.AudioClip) ? overrides.AudioClip : AudioClip;
                Volume = overrideField.HasFlag(Overrides.Volume) ? overrides.Volume : Volume;
                Pitch = overrideField.HasFlag(Overrides.Pitch) ? overrides.Pitch : Pitch;
            }

            public override string ToString() => $"AudioSnapshot({AudioClip},vol={Volume},pitch={Pitch})";
        }
        [Serializable]
        public struct Fuzz : Snapshots.IFuzz<Snapshot, Overrides>
        {
            public float Volume;
            public float Pitch;

            public void Apply(Snapshots.IFuzzControls controls, Overrides fuzzFields, in Snapshot start, ref Snapshot target)
            {
                target.Volume += fuzzFields.HasFlag(Overrides.Volume) ? controls.Power + controls.Random.Range(-Volume, Volume) : 0f;
                target.Pitch += fuzzFields.HasFlag(Overrides.Pitch) ? controls.Speed + controls.Random.Range(-Pitch, Pitch) : 0f;
            }
        }
        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Snapshot GetLocalSnapshot(this AudioSource thiz)
        => new()
        {
            AudioClip = thiz.clip,
            Volume = thiz.volume,
            Pitch = thiz.pitch,
        };
        /// Consider using GetLocalSnapshot to figure out which fields you _don't want to set_ first...
        public static void SetFromLocalSnapshot(this AudioSource thiz, Snapshot target)
        {
            thiz.pitch = target.Pitch;
            if (target.AudioClip != null) thiz.PlayOneShot(target.AudioClip, target.Volume);
            else thiz.volume = target.Volume;
        }
    }
}
