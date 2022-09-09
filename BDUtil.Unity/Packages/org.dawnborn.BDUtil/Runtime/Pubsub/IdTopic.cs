using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/IdTopic")]
    [Tooltip("Topic of Id->GameObject")]
    public class IdTopic : CollectionTopic<Observable.Dictionary<int, GameObject>, KeyValuePair<int, GameObject>, KVP.Entry<int, GameObject>> { }
}