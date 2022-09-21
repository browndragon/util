using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/FloatTopic")]
    public class FloatTopic : ValueTopic<float>
    {
        [Serializable]
        public class UEventFloat : Subscribers.UEventValue<float> { }
    }
}