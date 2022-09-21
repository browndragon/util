using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/RectTopic")]
    public class RectTopic : ValueTopic<Rect>
    {
        [Serializable]
        public class UEventRect : Subscribers.UEventValue<Rect> { }
    }
}