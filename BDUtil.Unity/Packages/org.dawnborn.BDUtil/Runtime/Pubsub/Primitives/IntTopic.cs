using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/IntTopic")]
    public class IntTopic : ValueTopic<int>
    {
        [Serializable]
        public class UEventInt : Subscribers.UEventValue<int> { }
    }
}