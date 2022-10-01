using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Geom/RectTopic", order = +5)]
    [Bind.Impl(typeof(ValueTopic<Rect>))]
    public class RectTopic : ValueTopic<Rect>
    {
        [Serializable]
        public class UEventRect : Subscribers.UEventValue<Rect> { }
    }
}