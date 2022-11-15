using System;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{
    [CreateAssetMenu(menuName = "BDUtil/Library/Prefab")]
    public class PrefabLibrary : Library<PrefabLibrary.Target, GameObject>, Player.IPlayerLibrary
    {
        [Tooltip("If true, apply player rotation to spawns")]
        public bool ApplyPlayerRotation = true;
        [Tooltip("If true, apply player scale to spawns")]
        public bool ApplyPlayerScale = false;
        protected override bool IsEntryForObject(in Target entry, GameObject obj)
        => entry.Prefab == obj;

        protected override Target NewEntry(Target template, GameObject fromObj)
        {
            template.Prefab = fromObj;
            return template;
        }

        [Serializable]
        public struct Target
        {
            public Interval Delay;
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
        public void Validate(Player player) { }
        public bool Play(Player player)
        {
            Target spawn = (Target)player.Chooser.ChooseNext(this).Data;
            Transforms.Snapshot originalPrefab = new(spawn.Prefab.transform);

            Transforms.Snapshot playerAsParent = new(player.transform);
            if (!ApplyPlayerRotation) playerAsParent.EulerAngles = Vector3.zero;
            if (!ApplyPlayerScale) playerAsParent.Scale = Vector3.one;

            Transforms.Snapshot resultant = playerAsParent.Matrix * Randoms.main.Range(spawn.TransformTarget).Matrix;
            resultant.ApplyTo(spawn.Prefab.transform);
            switch (spawn.Strategy)
            {
                case Target.Strategies.Instantiate:
                    Instantiate(spawn.Prefab);
                    break;
                case Target.Strategies.PoolAcquire:
                    Clone.Pool.main.Acquire(spawn.Prefab, true);
                    break;
                case Target.Strategies.PoolAcquireChildren:
                    // Contextualizes the children under the relativeTo.
                    Clone.Pool.main.AcquireViaAssetsOfChildren(spawn.Prefab.transform, null, true);
                    break;
            }
            originalPrefab.ApplyTo(spawn.Prefab.transform);
            Debug.Log($"Spawned {originalPrefab.Position} under {playerAsParent.Position}=>{resultant.Position}, then restored to {spawn.Prefab.transform.position}");
            player.Delay.Reset(Randoms.main.Range(spawn.Delay));
            return true;
        }
    }
}
