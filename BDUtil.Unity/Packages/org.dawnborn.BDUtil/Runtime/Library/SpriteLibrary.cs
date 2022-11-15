using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Sprite")]
    public class SpriteLibrary : Library<SpriteRenderers.Target, Sprite>, Player.IPlayerLibrary
    {
        public bool Play(Player player)
        {
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            SpriteRenderers.Target target = (SpriteRenderers.Target)player.Chooser.ChooseNext(this).Data;
            SpriteRenderers.Snapshot final = Randoms.main.Range(target);
            final.ApplyTo(renderer);
            return true;
        }

        public void Validate(Player player)
        {
            if (player.GetComponentInChildren<SpriteRenderer>()) return;
            Debug.LogWarning($"{player} doesn't have sprite renderer for {this}", player);
        }

        protected override bool IsEntryForObject(in SpriteRenderers.Target entry, Sprite obj)
        => entry.Sprite == obj;

        protected override SpriteRenderers.Target NewEntry(SpriteRenderers.Target template, Sprite fromObj)
        {
            template.Sprite = fromObj;
            return template;
        }

    }
}
