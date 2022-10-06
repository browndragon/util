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
        protected override SpriteRenderers.Snapshot Get(Player player)
        => player.spriteRenderer.GetLocalSnapshot();
        protected override void Set(Player player, SpriteRenderers.Snapshot local)
        => player.spriteRenderer.SetFromLocalSnapshot(local);
    }
}
