using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/BoundsTopic")]
    public class BoundsTopic : ValueTopic<Bounds>
    {
        [Serializable]
        public class UEventBounds : Subscribers.UEventValue<Bounds> { }
    }
}