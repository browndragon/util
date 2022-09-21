using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V3ITopic")]
    public class V3ITopic : ValueTopic<Vector3Int>
    {
        [Serializable]
        public class UEventV3I : Subscribers.UEventValue<Vector3Int> { }
    }
}