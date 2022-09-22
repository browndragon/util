using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/OTopic")]
    [Tooltip("Arbitrary object-passing topic. You probably want something more specific (ObjsHead? ObjsSet?)")]
    [Bind.Impl(typeof(ValueTopic<object>))]
    public class OTopic : ValueTopic<object>
    {
        [Serializable]
        public class UEventObject : Subscribers.UEventValue<object> { }
    }
}