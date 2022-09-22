using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/BoundsIntTopic")]
    [Bind.Impl(typeof(ValueTopic<BoundsInt>))]
    public class BoundsIntTopic : ValueTopic<BoundsInt>
    {
        [Serializable]
        public class UEventBoundsInt : Subscribers.UEventValue<BoundsInt> { }
    }
}