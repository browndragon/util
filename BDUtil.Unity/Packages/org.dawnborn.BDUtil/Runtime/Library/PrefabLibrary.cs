using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Library
{
    [CreateAssetMenu(menuName = "BDUtil/Library/Prefab")]
    public class PrefabLibrary : Library<PrefabLibrary.Spawn>
    {
        [Serializable]
        public struct Spawn : Player.IPlayable
        {
            public GameObject Prefab;
            [MinMax.Range(Max = 10f)] public MinMax Delay;
            public float PlayOn(Player player)
            {
                GameObject instance = Clone.Pool.main.Acquire(Prefab, false);
                instance.transform.position = player.transform.position;
                instance.transform.rotation = player.transform.rotation;
                instance.SetActive(true);
                return Delay.Random;
            }
        }
    }
}
