using System;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{
    public abstract class PlayerLibrary<TObj, TSnapshot, TTarget> : Library<TObj, Snapshots.Animate<TSnapshot, TTarget>>
    where TSnapshot : Snapshots.ISnapshot<TSnapshot>
    where TTarget : Snapshots.ITarget<TSnapshot>
    {
        protected override bool IsEntryForObject(in Snapshots.Animate<TSnapshot, TTarget> data, TObj obj)
        => IsEntryForTarget(data.Target, obj);
        protected abstract bool IsEntryForTarget(in TTarget data, TObj obj);
        protected override Entry NewEntry(Entry template, TObj fromObj)
        {
            var animate = template.Data;
            var target = animate.Target;
            target = NewTarget(target, fromObj);
            animate.Target = target;
            template.Data = animate;
            return template;
        }
        protected abstract TTarget NewTarget(TTarget snapshot, TObj fromObj);
        protected virtual float TotalDuration(Snapshots.IFuzzControls player, Snapshots.Animate<TSnapshot, TTarget> animate)
        => player.Random.Fuzzed(animate.Delay) / player.Speed;
        protected abstract TSnapshot GetInitial(Snapshots.IFuzzControls player);
        protected abstract TSnapshot Get(Snapshots.IFuzzControls player);
        protected abstract void Set(Snapshots.IFuzzControls player, TSnapshot local);
        protected override float Play(Snapshots.IFuzzControls player, Snapshots.Animate<TSnapshot, TTarget> animate)
        {
            float duration = TotalDuration(player, animate);
            var start = Get(player);
            var target = animate.Target.GetTarget(player, GetInitial(player));
            player.StartCoroutine(Clock.Now.StoppedDelayOf(duration).Foreach(t =>
                {
                    var eased = animate.Easing.Invoke(t);
                    var current = Get(player);
                    var currentTarget = current;
                    currentTarget.Override(target);  // Override _again_ to fix any NaNs in the target.
                    var lerped = start.Lerp(currentTarget, eased);
                    Set(player, lerped);
                }
            ));
            return duration;
        }
    }
    public abstract class PlayerLibrary<TSnapshot, TTarget> : PlayerLibrary<Void, TSnapshot, TTarget>
    where TSnapshot : Snapshots.ISnapshot<TSnapshot>
    where TTarget : Snapshots.ITarget<TSnapshot>
    {
        protected override bool HasEntryForObject(Void obj) => false;
        protected override bool IsEntryForObject(in Snapshots.Animate<TSnapshot, TTarget> data, Void obj)
        => false;
        protected override bool IsEntryForTarget(in TTarget data, Void obj)
        => false;
        protected override Entry NewEntry(Entry template, Void fromObj) => template;
        protected override TTarget NewTarget(TTarget snapshot, Void fromObj) => snapshot;
    }
}
