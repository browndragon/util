using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V3ITopic", order = +1)]
    [Bind.Impl(typeof(ValueTopic<Vector3Int>))]
    public class V3ITopic : ValueTopic<Vector3Int>
    {
        [Serializable]
        public class UEventV3I : Subscribers.UEventValue<Vector3Int> { }
    }
}