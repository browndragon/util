using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/IntTopic")]
    [Bind.Impl(typeof(ValueTopic<int>))]
    public class IntTopic : ValueTopic<int>
    {
        [Serializable]
        public class UEventInt : Subscribers.UEventValue<int> { }
    }
}