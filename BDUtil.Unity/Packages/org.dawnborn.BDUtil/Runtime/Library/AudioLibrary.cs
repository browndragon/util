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
        protected override float TotalDuration(Snapshots.IFuzzControls player, Snapshots.Animate<AudioSources.Snapshot, AudioSources.Overrides, AudioSources.Fuzz> animate)
        => ((animate.FuzzTarget.Pivot.AudioClip?.length ?? 0f) + player.Random.RandomValue(animate.Delay)) / player.Speed;

        protected override AudioSources.Snapshot Get(Snapshots.IFuzzControls player)
        => player.audio.GetLocalSnapshot();
        protected override AudioSources.Snapshot GetInitial(Snapshots.IFuzzControls player)
        => player.audioSnapshot;
        protected override void Set(Snapshots.IFuzzControls player, AudioSources.Snapshot local)
        => player.audio.SetFromLocalSnapshot(local);
    }
}
