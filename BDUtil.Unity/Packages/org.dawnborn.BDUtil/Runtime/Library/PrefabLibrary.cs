using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Library
{
    [CreateAssetMenu(menuName = "BDUtil/Library/Prefab")]
    public class PrefabLibrary : Library<GameObject, PrefabLibrary.Spawn>
    {
        [Serializable]
        public struct Spawn : Player.IPlayable<GameObject>
        {
            public AnimationCurve Delay;
            public float PlayOn(Player player, GameObject prefab)
            {
                GameObject instance = Clone.Pool.main.Acquire(prefab, false);
                instance.transform.position = player.transform.position;
                instance.transform.rotation = player.transform.rotation;
                instance.SetActive(true);
                return Delay.RandomValue();
            }
        }
    }
}
