using System;
using BDUtil.Bind;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/Collider3D", order = +4)]
    [Impl(typeof(ValueTopic<Collider>))]
    public class Collider3DTopic : ValueTopic<Collider>
    {
        [Serializable]
        public class UEventCollider3D : Subscribers.UEventValue<Collider> { }
    }
}