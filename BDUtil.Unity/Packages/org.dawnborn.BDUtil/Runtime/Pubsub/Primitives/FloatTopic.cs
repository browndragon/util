using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/FloatTopic")]
    [Bind.Impl(typeof(ValueTopic<float>))]
    public class FloatTopic : ValueTopic<float>
    {
        [Serializable]
        public class UEventFloat : Subscribers.UEventValue<float> { }
    }
}