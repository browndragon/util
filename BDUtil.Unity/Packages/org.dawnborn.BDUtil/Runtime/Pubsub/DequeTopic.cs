using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/DequeTopic")]
    public class DequeTopic : IndexedCollectionTopic<Observable.Deque<GameObject>, int, GameObject, GameObject, GameObject> { }
}