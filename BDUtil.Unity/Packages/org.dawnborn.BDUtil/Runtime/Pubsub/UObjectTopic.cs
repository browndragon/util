using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/UObjectTopic")]
    public class UObjectTopic : MutableTopic<UnityEngine.Object, UnityEngine.Object> { }
}