using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/Collider2D")]
    public class Collider2DTopic : ValueTopic<Collider2D>
    {
        [Serializable]
        public class UEventCollider2D : Subscribers.UEventValue<Collider2D> { }
    }
}