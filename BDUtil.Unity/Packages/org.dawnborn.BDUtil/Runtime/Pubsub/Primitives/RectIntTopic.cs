using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Geom/RectIntTopic", order = +5)]
    [Bind.Impl(typeof(ValueTopic<RectInt>))]
    public class RectIntTopic : ValueTopic<RectInt>
    {
        [Serializable]
        public class UEventRectInt : Subscribers.UEventValue<RectInt> { }
    }
}