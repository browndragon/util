using System;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/Deque")]
    public class ObjsDeque : CollectionTopic<Observable.Deque<GameObject>, GameObject> { }
}