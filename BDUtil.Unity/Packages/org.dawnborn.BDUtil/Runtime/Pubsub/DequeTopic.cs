using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/DequeTopic")]
    public class DequeTopic : CollectionTopic<Observable.Deque<GameObject>, GameObject, GameObject> { }
}