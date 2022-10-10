using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Sprite")]
    public class SpriteLibrary : PlayerLibrary<Sprite, SpriteRenderers.Snapshot, SpriteRenderers.Target>
    {
        protected override bool IsEntryForTarget(in SpriteRenderers.Target entry, Sprite obj)
        => entry.TargetSprite.HasValue && entry.TargetSprite.Value == obj;

        protected override SpriteRenderers.Target NewTarget(SpriteRenderers.Target template, Sprite fromObj)
        {
            template.TargetSprite = fromObj;
            return template;
        }
        protected override SpriteRenderers.Snapshot Get(Snapshots.IFuzzControls player)
        => player.renderer.GetLocalSnapshot();
        protected override SpriteRenderers.Snapshot GetInitial(Snapshots.IFuzzControls player)
        => player.rendererSnapshot;
        protected override void Set(Snapshots.IFuzzControls player, SpriteRenderers.Snapshot local)
        => player.renderer.SetFromLocalSnapshot(local);
    }
}
