using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/Set")]
    public class ObjsSet : CollectionTopic<Observable.Set<GameObject>, GameObject> { }
}