using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/RectTopic")]
    [Bind.Impl(typeof(ValueTopic<Rect>))]
    public class RectTopic : ValueTopic<Rect>
    {
        [Serializable]
        public class UEventRect : Subscribers.UEventValue<Rect> { }
    }
}