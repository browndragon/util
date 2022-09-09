using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/SetTopic")]
    public class SetTopic : CollectionTopic<Observable.Set<GameObject>, GameObject, GameObject> { }
}