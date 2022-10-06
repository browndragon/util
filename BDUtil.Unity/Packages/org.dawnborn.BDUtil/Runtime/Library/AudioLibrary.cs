using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Audio")]
    public class AudioLibrary : PlayerLibrary<AudioClip, AudioSources.Snapshot, AudioSources.Overrides, AudioSources.Fuzz>
    {
        protected override bool IsEntryForObject(in AudioSources.Snapshot entry, AudioClip obj)
        => entry.AudioClip == obj;

        protected override AudioSources.Snapshot NewEntry(AudioSources.Snapshot template, AudioClip fromObj)
        {
            template.AudioClip = fromObj;
            return template;
        }
        protected override float TotalDuration(Player player, Snapshots.Animate<AudioSources.Snapshot, AudioSources.Overrides, AudioSources.Fuzz> animate)
        => ((animate.FuzzTarget.Pivot.AudioClip?.length ?? 0f) + player.Random.RandomValue(animate.Delay)) / player.Speed;

        protected override AudioSources.Snapshot Get(Player player)
        => player.audioSource.GetLocalSnapshot();

        protected override void Set(Player player, AudioSources.Snapshot local)
        => player.audioSource.SetFromLocalSnapshot(local);
    }
}
