using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V/3Topic", order = +1)]
    [Bind.Impl(typeof(ValueTopic<Vector3>))]
    public class V3Topic : ValueTopic<Vector3>
    {
        [Serializable]
        public class UEventV3 : Subscribers.UEventValue<Vector3> { }
    }
}