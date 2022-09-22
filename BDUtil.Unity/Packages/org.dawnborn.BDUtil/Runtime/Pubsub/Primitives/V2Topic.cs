using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V2Topic")]
    [Bind.Impl(typeof(ValueTopic<Vector2>))]
    public class V2Topic : ValueTopic<Vector2>
    {
        [Serializable]
        public class UEventV2 : Subscribers.UEventValue<Vector2> { }
    }
}