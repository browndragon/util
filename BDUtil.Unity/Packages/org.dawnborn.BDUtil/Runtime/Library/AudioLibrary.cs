using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Audio")]
    public class AudioLibrary : Library<AudioClip, AudioLibrary.AudioClipParams>
    {
        protected override bool IsEntryForObject(in AudioClipParams entry, AudioClip obj)
        => entry.AudioClip == obj;

        protected override Entry NewEntry(Entry template, AudioClip fromObj)
        {
            AudioClipParams @params = template.Data;
            @params.AudioClip = fromObj;
            template.Data = @params;
            return template;
        }

        [Serializable]
        public struct AudioClipParams : Player.IPlayable
        {
            public Extent Delay;
            public Extent Volume;
            public Extent Pitch;
            public AudioClip AudioClip;
            public float PlayOn(Player player)
            {
                float delay = Delay.ScaledBy(player.Chaos).RandomPoint() / player.Speed;
                if (AudioClip != null)
                {
                    AudioSource source = player.GetComponent<AudioSource>().OrThrow();
                    source.pitch = Pitch.ScaledBy(player.Chaos, player.Speed).RandomPoint();
                    source.PlayOneShot(AudioClip, player.Power + Volume.ScaledBy(player.Chaos).RandomPoint());
                    delay += AudioClip.length;
                }
                return delay;
            }
        }
    }
}
