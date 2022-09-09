using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Joiner")]
    [Tooltip("OnEnable joins all SetTopics as a member (!not! subscriber); OnDisable, leaves them.")]
    public class Joiner : MonoBehaviour
    {
        [SerializeField] SetTopic[] Topics;
        [SerializeField] IdTopic[] IdTopics;
        readonly Disposes.All unsubscribe = new();
        void OnEnable()
        {
            foreach (SetTopic topic in Topics)
            {
                topic.Collection.Add(gameObject);
                unsubscribe.Add(() => topic.Collection.Remove(gameObject));
            }
            foreach (IdTopic topic in IdTopics)
            {
                topic.Collection.Add(gameObject.GetInstanceID(), gameObject);
                unsubscribe.Add(() => topic.Collection.Remove(gameObject.GetInstanceID()));
            }
        }
        void OnDisable() => unsubscribe.Dispose();
    }
}