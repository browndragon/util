using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V2Topic")]
    public class V2Topic : ValueTopic<Vector2>
    {
        [Serializable]
        public class UEventV2 : Subscribers.UEventValue<Vector2> { }
    }
}