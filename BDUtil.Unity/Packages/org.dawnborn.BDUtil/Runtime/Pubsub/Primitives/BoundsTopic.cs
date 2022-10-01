using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/BoundsTopic", order = +1)]
    [Bind.Impl(typeof(ValueTopic<Bounds>))]
    public class BoundsTopic : ValueTopic<Bounds>
    {
        [Serializable]
        public class UEventBounds : Subscribers.UEventValue<Bounds> { }
    }
}