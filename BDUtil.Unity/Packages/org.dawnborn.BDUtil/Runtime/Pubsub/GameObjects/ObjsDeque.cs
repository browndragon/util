using System;
using BDUtil.Bind;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/Deque", order = +4)]
    [Impl(typeof(CollectionTopic<Observable.Deque<GameObject>>))]
    public class ObjsDeque : CollectionTopic<Observable.Deque<GameObject>, GameObject> { }
}