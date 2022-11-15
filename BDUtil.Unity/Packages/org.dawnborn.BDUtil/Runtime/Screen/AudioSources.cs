using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class AudioSources
    {
        [Serializable]
        public struct Snapshot : ISnapshot<AudioSource>
        {
            public AudioClip AudioClip;
            public float Volume;
            public float Pitch;
            public bool Loop;

            public Snapshot(AudioClip audioClip, float volume, float pitch, bool loop)
            {
                AudioClip = audioClip;
                Volume = volume;
                Pitch = pitch;
                Loop = loop;
            }
            public Snapshot(AudioSource player) : this(player.clip, player.volume, player.pitch, player.loop) { }

            public void ReadFrom(AudioSource player)
            => this = new(player);

            public void ReadFrom(GameObject player) => ReadFrom(player.GetComponent<AudioSource>());
            public void ApplyTo(AudioSource player)
            {
                player.clip = AudioClip;
                player.volume = Volume;
                player.pitch = Pitch;
                player.loop = Loop;
            }

            public void ApplyTo(GameObject player) => ApplyTo(player.GetComponent<AudioSource>());
            public override string ToString() => $"AudioSnapshot({AudioClip},vol={Volume},pitch={Pitch})";
        }
    }
}
