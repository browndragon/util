using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/OTopic")]
    [Tooltip("Arbitrary object-passing topic. You probably want something more specific (ObjsHead? ObjsSet?)")]
    public class OTopic : ValueTopic<object> { }
}