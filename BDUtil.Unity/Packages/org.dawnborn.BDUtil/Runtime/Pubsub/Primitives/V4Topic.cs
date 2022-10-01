using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V/4Topic", order = +1)]
    [Bind.Impl(typeof(ValueTopic<Vector4>))]
    public class V4Topic : ValueTopic<Vector4>
    {
        [Serializable]
        public class UEventV4 : Subscribers.UEventValue<Vector4> { }
    }
}