using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Transform")]
    public class TransformLibrary : PlayerLibrary<Transforms.Local, Transforms.Overrides, Transforms.Fuzz>
    {
        protected override Transforms.Local Get(Player player)
        => player.transform.GetLocalSnapshot();
        protected override void Set(Player player, Transforms.Local local)
        => player.transform.SetFromLocalSnapshot(local);
    }
}
