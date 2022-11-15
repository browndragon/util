using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Audio")]
    public class AudioLibrary : Library<AudioLibrary.Target, AudioClip>, Player.IPlayerLibrary
    {
        [Serializable]
        public struct Target
        {
            public AudioClip AudioClip;
            public Interval Volume;
            public Interval Pitch;
            public Interval Delay;
            [Range(0, 1)] public float Loop;
        }
        public float ScaleVolume = 1f;
        public bool Play(Player player)
        {
            AudioSource audioSource = player.GetComponent<AudioSource>();
            Entry entry = (Entry)player.Chooser.ChooseNext(this);
            bool loop;
            AudioSources.Snapshot target = new(
                entry.Data.AudioClip,
                ScaleVolume * Randoms.main.Range(entry.Data.Volume),
                Randoms.main.Range(entry.Data.Pitch),
                loop = Randoms.main.Odds(entry.Data.Loop)
            );
            target.ApplyTo(audioSource);
            if (loop) player.Delay.Reset(float.PositiveInfinity);
            else player.Delay.Reset(audioSource.clip.length * audioSource.pitch + Randoms.main.Range(entry.Data.Delay));
            return true;
        }
        public void Validate(Player player)
        {
            AudioSource audioSource = player.GetComponent<AudioSource>();
            if (!audioSource)
            {
                Debug.LogWarning($"{player} has {this} audio lib but no audio source!", player);
            }
        }

        protected override bool IsEntryForObject(in Target entry, AudioClip obj)
        => entry.AudioClip == obj;

        protected override Target NewEntry(Target template, AudioClip fromObj)
        {
            template.AudioClip = fromObj;
            return template;
        }
    }
}
