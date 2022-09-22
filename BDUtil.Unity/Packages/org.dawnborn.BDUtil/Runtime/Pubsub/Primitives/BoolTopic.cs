using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/BoolTopic")]
    [Bind.Impl(typeof(ValueTopic<bool>))]
    public class BoolTopic : ValueTopic<bool>
    {
        [Serializable]
        public class UEventBool : Subscribers.UEventValue<bool> { }
    }
}