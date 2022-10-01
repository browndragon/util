using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V/RectTopic", order = +1)]
    [Bind.Impl(typeof(ValueTopic<Rect>))]
    public class RectTopic : ValueTopic<Rect>
    {
        [Serializable]
        public class UEventRect : Subscribers.UEventValue<Rect> { }
    }
}