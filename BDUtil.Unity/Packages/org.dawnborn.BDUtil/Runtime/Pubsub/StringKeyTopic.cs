using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/StringKeyTopic")]
    [Tooltip("A string-keyed 'registry' type; updates are observable (at top level only!); values are arbitrary objects")]
    // TODO: I'd guess this breaks in the inspector, since objects just aren't serializable, even using entries.
    public class StringKeyTopic : CollectionTopic<Observable.Dictionary<string, object>, KeyValuePair<string, object>, KVP.Entry<string, object>> { }
}