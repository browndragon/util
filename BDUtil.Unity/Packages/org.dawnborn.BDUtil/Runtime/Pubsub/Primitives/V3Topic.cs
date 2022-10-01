using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Geom/3Topic", order = +5)]
    [Bind.Impl(typeof(ValueTopic<Vector3>))]
    public class V3Topic : ValueTopic<Vector3>
    {
        [Serializable]
        public class UEventV3 : Subscribers.UEventValue<Vector3> { }
    }
}