using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Audio")]
    public class AudioLibrary : PlayerLibrary<AudioClip, AudioSources.Snapshot, AudioSources.Target>
    {
        protected override bool IsEntryForTarget(in AudioSources.Target entry, AudioClip obj)
        => entry.AudioClip.HasValue && entry.AudioClip.Value == obj;

        protected override AudioSources.Target NewTarget(AudioSources.Target template, AudioClip fromObj)
        {
            template.AudioClip = fromObj;
            return template;
        }
        protected override float TotalDuration(Snapshots.IFuzzControls player, Snapshots.Animate<AudioSources.Snapshot, AudioSources.Target> animate)
        => ((animate.Target.AudioClip.GetValueOrDefault()?.length ?? 0f) + player.Random.Fuzzed(animate.Delay)) / player.Speed;

        protected override AudioSources.Snapshot Get(Snapshots.IFuzzControls player)
        => player.audio.GetLocalSnapshot();
        protected override AudioSources.Snapshot GetInitial(Snapshots.IFuzzControls player)
        => player.audioSnapshot;
        protected override void Set(Snapshots.IFuzzControls player, AudioSources.Snapshot local)
        => player.audio.SetFromLocalSnapshot(local);
    }
}
