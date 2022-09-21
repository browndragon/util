using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V3Topic")]
    public class V3Topic : ValueTopic<Vector3>
    {
        [Serializable]
        public class UEventV3 : Subscribers.UEventValue<Vector3> { }
    }
}