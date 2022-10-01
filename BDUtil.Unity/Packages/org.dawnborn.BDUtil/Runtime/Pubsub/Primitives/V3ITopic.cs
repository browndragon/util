using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Geom/3ITopic", order = +5)]
    [Bind.Impl(typeof(ValueTopic<Vector3Int>))]
    public class V3ITopic : ValueTopic<Vector3Int>
    {
        [Serializable]
        public class UEventV3I : Subscribers.UEventValue<Vector3Int> { }
    }
}