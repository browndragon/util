using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V/BoundsIntTopic", order = +1)]
    [Bind.Impl(typeof(ValueTopic<BoundsInt>))]
    public class BoundsIntTopic : ValueTopic<BoundsInt>
    {
        [Serializable]
        public class UEventBoundsInt : Subscribers.UEventValue<BoundsInt> { }
    }
}