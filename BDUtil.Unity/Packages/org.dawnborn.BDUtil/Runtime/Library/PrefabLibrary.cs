using System;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

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
            public Randoms.Fuzzy<float> Delay;
            public GameObject Prefab;
            public enum Strategies
            {
                PoolAcquire = default,
                Instantiate,  // OOF BUDDY
                PoolAcquireChildren,
            }
            public Strategies Strategy;
            public Transforms.Target TransformTarget;
        }
        protected override float Play(Snapshots.IFuzzControls player, Spawn spawn)
        {
            Transforms.Snapshot prefab = spawn.Prefab.transform.GetLocalSnapshot();
            Transforms.Snapshot local = player.transform.GetLocalSnapshot();
            local.scale0 = Vector3.zero;
            spawn.Prefab.transform.SetFromLocalSnapshot(
                 local.Matrix * spawn.TransformTarget.GetTarget(player, prefab).Matrix
            );
            switch (spawn.Strategy)
            {
                case Spawn.Strategies.Instantiate:
                    Instantiate(spawn.Prefab);
                    break;
                case Spawn.Strategies.PoolAcquire:
                    Clone.Pool.main.Acquire(spawn.Prefab, true);
                    break;
                case Spawn.Strategies.PoolAcquireChildren:
                    // Contextualizes the children under the relativeTo.
                    Clone.Pool.main.AcquireViaAssetsOfChildren(spawn.Prefab.transform, null, true);
                    break;
            }
            spawn.Prefab.transform.SetFromLocalSnapshot(prefab);
            return player.Random.Fuzzed(spawn.Delay) / player.Speed;
        }
    }
}
