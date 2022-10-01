using System;
using BDUtil.Bind;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/GameObject", order = +4)]
    [Impl(typeof(ValueTopic<GameObject>))]
    public class GameObjectTopic : ValueTopic<GameObject>
    {
        [Serializable]
        public class UEventGameObject : Subscribers.UEventValue<GameObject> { }
    }
}