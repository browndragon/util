using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Audio")]
    public class AudioLibrary : Library<AudioLibrary.Clip>
    {
        public float VolumeScale;
        [Serializable]
        public struct Clip : Player.IPlayable
        {
            // Formerly used.
            [SerializeField, MinMax.Range] Vector2 volume;
            [SerializeField, MinMax.Range(Max = 10f)] Vector2 delay;
            public AudioClip AudioClip;

            public float PlayOn(Player player)
            {
                AudioSource source = player.GetComponent<AudioSource>().OrThrow();
                float delay = ((MinMax)this.delay).Random;
                if (AudioClip != null)
                {
                    source.clip = AudioClip;
                    float scale = DefaultSafe(((AudioLibrary)player.Library).VolumeScale);
                    source.volume = scale * ((MinMax)this.volume).Random;
                    delay += AudioClip.length;
                    source.Play();
                }
                return delay;
            }
        }
    }
}
