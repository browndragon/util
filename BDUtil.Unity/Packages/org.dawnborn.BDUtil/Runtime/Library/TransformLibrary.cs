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
        protected override Transforms.Local Get(ILibraryPlayer player)
        => player.transform.GetLocalSnapshot();
        protected override void Set(ILibraryPlayer player, Transforms.Local local)
        => player.transform.SetFromLocalSnapshot(local);
    }
}
