using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/BoundsIntTopic")]
    public class BoundsIntTopic : ValueTopic<BoundsInt>
    {
        [Serializable]
        public class UEventBoundsInt : Subscribers.UEventValue<BoundsInt> { }
    }
}