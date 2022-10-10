using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Transform")]
    public class TransformLibrary : PlayerLibrary<Transforms.Snapshot, Transforms.Target>
    {
        protected override Transforms.Snapshot GetInitial(Snapshots.IFuzzControls player)
        => player.transformSnapshot;
        protected override Transforms.Snapshot Get(Snapshots.IFuzzControls player)
        => player.transform.GetLocalSnapshot();
        protected override void Set(Snapshots.IFuzzControls player, Transforms.Snapshot local)
        => player.transform.SetFromLocalSnapshot(local);
    }
}
