using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{

    [CreateAssetMenu(menuName = "BDUtil/Library/Transform")]
    public class TransformLibrary : Library<Transforms.Target>, Player.IPlayerLibrary
    {
        public bool PositionAdditive;
        public bool RotationAdditive;
        public bool Play(Player player)
        {
            Transforms.Snapshot initial = new(player.transform);
            Transforms.Target target = (Transforms.Target)player.Chooser.ChooseNext(this).Data;
            Transforms.Snapshot final = Randoms.main.Range(target);
            if (PositionAdditive) final.Position += initial.Position;
            if (RotationAdditive) final.Rotation *= initial.Rotation;
            final.ApplyTo(player.transform);
            return true;
        }

        public void Validate(Player player) { }
    }
}
