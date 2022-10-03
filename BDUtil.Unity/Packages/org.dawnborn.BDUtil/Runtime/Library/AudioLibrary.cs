using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Audio")]
    public class AudioLibrary : Library<AudioClip, AudioLibrary.AudioClipParams>
    {
        public float VolumeScale;
        [Serializable]
        public struct AudioClipParams : Player.IPlayable<AudioClip>
        {
            public Extent Volume;
            public Extent Delay;
            public float PlayOn(Player player, AudioClip audioClip)
            {
                float delay = Delay.RandomPoint();
                if (audioClip != null)
                {
                    AudioSource source = player.GetComponent<AudioSource>().OrThrow();
                    source.clip = audioClip;
                    float scale = DefaultSafe(((AudioLibrary)player.Library).VolumeScale);
                    source.volume = scale * Volume.RandomPoint();
                    delay += audioClip.length;
                    source.Play();
                }
                return delay;
            }
        }
    }
}
