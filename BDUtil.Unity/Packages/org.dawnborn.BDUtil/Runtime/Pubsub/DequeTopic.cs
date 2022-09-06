using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/DequeTopic")]
    public class DequeTopic : MutableTopic<OrderedDeque<object>, IReadOnlyDeque<object>> { }
}