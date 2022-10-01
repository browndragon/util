using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Objs/Set", order = +4)]
    public class ObjsSet : CollectionTopic<Observable.Set<GameObject>, GameObject> { }
}