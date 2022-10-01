using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/BoolTopic", order = +1)]
    [Bind.Impl(typeof(ValueTopic<bool>))]
    public class BoolTopic : ValueTopic<bool>
    {
        [Serializable]
        public class UEventBool : Subscribers.UEventValue<bool> { }
    }
}