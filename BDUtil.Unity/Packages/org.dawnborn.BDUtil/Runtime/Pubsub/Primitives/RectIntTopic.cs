using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/RectIntTopic")]
    [Bind.Impl(typeof(ValueTopic<RectInt>))]
    public class RectIntTopic : ValueTopic<RectInt>
    {
        [Serializable]
        public class UEventRectInt : Subscribers.UEventValue<RectInt> { }
    }
}