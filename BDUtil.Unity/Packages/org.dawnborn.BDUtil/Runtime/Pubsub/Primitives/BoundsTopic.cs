using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/BoundsTopic")]
    [Bind.Impl(typeof(ValueTopic<Bounds>))]
    public class BoundsTopic : ValueTopic<Bounds>
    {
        [Serializable]
        public class UEventBounds : Subscribers.UEventValue<Bounds> { }
    }
}