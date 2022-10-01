using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Geom/2Topic", order = +5)]
    [Bind.Impl(typeof(ValueTopic<Vector2>))]
    public class V2Topic : ValueTopic<Vector2>
    {
        [Serializable]
        public class UEventV2 : Subscribers.UEventValue<Vector2> { }
    }
}