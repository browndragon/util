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
            public Randoms.Fuzzed<float> Delay;
            public GameObject Prefab;
        }
        protected override float Play(Player player, Spawn spawn)
        {
            GameObject instance = Clone.Pool.main.Acquire(spawn.Prefab, false);
            instance.transform.position = player.transform.position;
            instance.transform.rotation = player.transform.rotation;
            instance.SetActive(true);
            return player.Random.RandomValue(spawn.Delay) / player.Speed;
        }

    }
}
