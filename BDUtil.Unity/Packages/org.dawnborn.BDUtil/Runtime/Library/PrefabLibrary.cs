using System;
using System.Collections.Generic;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;
using UnityEngine.UIElements;

namespace BDUtil.Library
{
    [CreateAssetMenu(menuName = "BDUtil/Library/Prefab")]
    public class PrefabLibrary : Library<GameObject, PrefabLibrary.Spawn>
    {
        protected override bool IsEntryForObject(in Spawn entry, GameObject obj)
        => entry.Prefab == obj;

        protected override Entry NewEntry(Entry template, GameObject fromObj)
        {
            Spawn spawn = template.Data;
            spawn.Prefab = fromObj;
            template.Data = spawn;
            return template;
        }

        [Serializable]
        public struct Spawn
        {
            public Randoms.Fuzzed<float> Delay;
            public GameObject Prefab;
            public enum Strategies
            {
                PoolAcquire = default,
                Instantiate,  // OOF BUDDY
                PoolAcquireChildren,
            }
            public Strategies Strategy;
            public enum RelativeTos
            {
                Player = default,
                MouseWorldZ0
            }
            public RelativeTos RelativeTo;
            public Snapshots.FuzzTarget<Transforms.Local, Transforms.Overrides, Transforms.Fuzz> Transform;
        }
        protected override float Play(Player player, Spawn spawn)
        {
            GameObject instance;
            Transforms.Local relativeTo = spawn.RelativeTo switch
            {
                Spawn.RelativeTos.Player => player.transform.GetLocalSnapshot(),
                Spawn.RelativeTos.MouseWorldZ0 => new()
                {
                    Position = player.camera.ScreenPointToIntersection(Input.mousePosition),
                    EulerAngles = default,
                    Scale = Vector3.one
                },
                _ => throw spawn.RelativeTo.BadValue(),
            };
            // However, we need to ignore our scale, which is just Not Helpful.
            // This isn't principled, it's just helpful.
            relativeTo.Scale = Vector3.one;
            relativeTo.Override(spawn.Transform.PivotOverrides, spawn.Transform.Pivot);
            spawn.Transform.Fuzz.Apply(player, spawn.Transform.TargetOverrides, relativeTo, ref relativeTo);

            Transforms.Local alreadyHad;
            Transforms.Local resultingIn;
            switch (spawn.Strategy)
            {
                case Spawn.Strategies.Instantiate:
                    instance = Instantiate(spawn.Prefab);
                    resultingIn = alreadyHad = instance.transform.GetLocalSnapshot();
                    resultingIn.ContextualizeUnder(relativeTo);
                    instance.transform.SetFromLocalSnapshot(resultingIn);
                    break;
                case Spawn.Strategies.PoolAcquire:
                    instance = Clone.Pool.main.Acquire(spawn.Prefab, true);
                    resultingIn = alreadyHad = instance.transform.GetLocalSnapshot();
                    resultingIn.ContextualizeUnder(relativeTo);
                    instance.transform.SetFromLocalSnapshot(resultingIn);
                    break;
                case Spawn.Strategies.PoolAcquireChildren:
                    // Contextualizes the children under the relativeTo.
                    Clone.Pool.main.AcquireViaAssetsOfChildren(spawn.Prefab.transform, null, relativeTo, true);
                    break;
            }
            return player.Random.RandomValue(spawn.Delay) / player.Speed;
        }

    }
}
