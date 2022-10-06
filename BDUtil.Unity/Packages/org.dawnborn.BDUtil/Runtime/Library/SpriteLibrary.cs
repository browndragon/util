using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Sprite")]
    public class SpriteLibrary : PlayerLibrary<Sprite, SpriteRenderers.Snapshot, SpriteRenderers.Overrides, SpriteRenderers.Fuzz>
    {
        protected override bool IsEntryForObject(in SpriteRenderers.Snapshot entry, Sprite obj)
        => entry.Sprite == obj;

        protected override SpriteRenderers.Snapshot NewEntry(SpriteRenderers.Snapshot template, Sprite fromObj)
        {
            template.Sprite = fromObj;
            return template;
        }
        protected override SpriteRenderers.Snapshot Get(ILibraryPlayer player)
        => player.renderer.GetLocalSnapshot();
        protected override void Set(ILibraryPlayer player, SpriteRenderers.Snapshot local)
        => player.renderer.SetFromLocalSnapshot(local);
    }
}
