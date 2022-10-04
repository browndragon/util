using System;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{
    [CreateAssetMenu(menuName = "BDUtil/Library/Prefab")]
    public class PrefabLibrary : Library<GameObject, PrefabLibrary.Spawn>
    {
        protected override bool IsEntryForObject(in PrefabLibrary.Spawn entry, GameObject obj)
        => entry.Prefab == obj;

        protected override Entry NewEntry(Entry template, GameObject fromObj)
        {
            Spawn spawn = template.Data;
            spawn.Prefab = fromObj;
            template.Data = spawn;
            return template;
        }

        [Serializable]
        public struct Spawn : Player.IPlayable
        {
            public Extent Delay;
            public GameObject Prefab;
            public float PlayOn(Player player)
            {
                GameObject instance = Clone.Pool.main.Acquire(Prefab, false);
                instance.transform.position = player.transform.position;
                instance.transform.rotation = player.transform.rotation;
                foreach (Player innerplayer in instance.GetComponents<Player>())
                {
                    innerplayer.Chaos = player.Chaos;
                    innerplayer.Power = player.Power;
                    innerplayer.Speed = player.Speed;
                }
                instance.SetActive(true);
                return Delay.ScaledBy(player.Chaos).RandomPoint() / player.Speed;
            }
        }
    }
}
