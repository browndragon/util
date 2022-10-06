using System;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{
    public abstract class PlayerLibrary<TObj, TSnapshot, TOverrides, TFuzz> : Library<TObj, Snapshots.Animate<TSnapshot, TOverrides, TFuzz>>
    where TSnapshot : Snapshots.ISnapshot<TSnapshot, TOverrides>
    where TOverrides : Enum
    where TFuzz : Snapshots.IFuzz<TSnapshot, TOverrides>
    {
        protected override bool IsEntryForObject(in Snapshots.Animate<TSnapshot, TOverrides, TFuzz> data, TObj obj)
        => IsEntryForObject(data.FuzzTarget.Pivot, obj);
        protected abstract bool IsEntryForObject(in TSnapshot data, TObj obj);
        protected override Entry NewEntry(Entry template, TObj fromObj)
        {
            var animate = template.Data;
            var fuzzTarget = animate.FuzzTarget;
            var pivot = fuzzTarget.Pivot;
            pivot = NewEntry(pivot, fromObj);
            fuzzTarget.Pivot = pivot;
            animate.FuzzTarget = fuzzTarget;
            template.Data = animate;
            return template;
        }
        protected abstract TSnapshot NewEntry(TSnapshot snapshot, TObj fromObj);
        protected virtual float TotalDuration(Player player, Snapshots.Animate<TSnapshot, TOverrides, TFuzz> animate)
        => player.Random.RandomValue(animate.Delay) / player.Speed;
        protected abstract TSnapshot Get(Player player);
        protected abstract void Set(Player player, TSnapshot local);
        protected override float Play(Player player, Snapshots.Animate<TSnapshot, TOverrides, TFuzz> animate)
        {
            float duration = TotalDuration(player, animate);
            var start = Get(player);
            var target = animate.FuzzTarget.GetTarget(player, start);
            player.StartCoroutine(new Timer(duration)
                .Foreach(t =>
                {
                    var eased = animate.Easing.Invoke(t);
                    var current = Get(player);
                    var lerped = start.Lerp(target, eased);
                    current.Override(animate.FuzzTarget.TargetOverrides, lerped);
                    Set(player, current);
                }));
            return duration;
        }
    }
    public abstract class PlayerLibrary<TSnapshot, TOverrides, TFuzz> : PlayerLibrary<Void, TSnapshot, TOverrides, TFuzz>
    where TSnapshot : Snapshots.ISnapshot<TSnapshot, TOverrides>
    where TOverrides : Enum
    where TFuzz : Snapshots.IFuzz<TSnapshot, TOverrides>
    {
        protected override bool HasEntryForObject(Void obj) => false;
        protected override bool IsEntryForObject(in Snapshots.Animate<TSnapshot, TOverrides, TFuzz> data, Void obj)
        => false;
        protected override bool IsEntryForObject(in TSnapshot data, Void obj)
        => false;
        protected override Entry NewEntry(Entry template, Void fromObj) => template;
        protected override TSnapshot NewEntry(TSnapshot snapshot, Void fromObj) => snapshot;
    }
}
