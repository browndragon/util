using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Geom/4Topic", order = +5)]
    [Bind.Impl(typeof(ValueTopic<Vector4>))]
    public class V4Topic : ValueTopic<Vector4>
    {
        [Serializable]
        public class UEventV4 : Subscribers.UEventValue<Vector4> { }
    }
}