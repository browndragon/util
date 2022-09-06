using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/SetTopic")]
    public class SetTopic : MutableTopic<HashSet<object>, IReadOnlyCollection<object>> { }
}