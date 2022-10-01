using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Geom/BoundsIntTopic", order = +5)]
    [Bind.Impl(typeof(ValueTopic<BoundsInt>))]
    public class BoundsIntTopic : ValueTopic<BoundsInt>
    {
        [Serializable]
        public class UEventBoundsInt : Subscribers.UEventValue<BoundsInt> { }
    }
}