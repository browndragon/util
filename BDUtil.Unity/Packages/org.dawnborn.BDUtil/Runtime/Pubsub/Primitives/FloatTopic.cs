using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/FloatTopic", order = +1)]
    [Bind.Impl(typeof(ValueTopic<float>))]
    public class FloatTopic : ValueTopic<float>
    {
        [Serializable]
        public class UEventFloat : Subscribers.UEventValue<float> { }
    }
}