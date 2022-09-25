using System;
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
            [SerializeField] Vector2 volume;
            public float MinVolume => DefaultSafe(volume.x);
            public float MaxVolume => DefaultSafe(volume.y, MinVolume);
            [SerializeField] Vector2 delay;
            public float MinDelay => DefaultSafe(delay.x);
            public float MaxDelay => DefaultSafe(delay.y, MinDelay);
            public AudioClip AudioClip;

            public float PlayOn(Player player)
            {
                AudioSource source = player.GetComponent<AudioSource>().OrThrow();
                float delay = UnityEngine.Random.Range(MinDelay, MaxDelay);
                if (AudioClip != null)
                {
                    source.clip = AudioClip;
                    float scale = DefaultSafe(((AudioLibrary)player.Library).VolumeScale);
                    source.volume = scale * UnityEngine.Random.Range(MinVolume, MaxVolume);
                    delay += AudioClip.length;
                    source.Play();
                }
                return delay;
            }
        }
    }
}
