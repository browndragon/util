using System;
using BDUtil.Bind;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/Collider2D", order = +4)]
    [Impl(typeof(ValueTopic<Collider2D>))]
    public class Collider2DTopic : ValueTopic<Collider2D>
    {
        [Serializable]
        public class UEventCollider2D : Subscribers.UEventValue<Collider2D> { }
    }
}