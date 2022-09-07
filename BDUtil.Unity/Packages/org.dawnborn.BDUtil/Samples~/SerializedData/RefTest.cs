using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil.Serialization
{
    public class RefTest : MonoBehaviour
    {
        public Ref<GameObject> GameObjectRef;
        public Ref<ScriptableObject> ScriptableObjectRef;
        public Ref<Transform> TransformRef;
        public Ref<IntTopic> IntTopicRef;
    }
}