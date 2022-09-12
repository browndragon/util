using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Joiner")]
    [Tooltip("OnEnable joins all SetTopics as a member (!not! subscriber); OnDisable, leaves them.")]
    public class Joiner : MonoBehaviour
    {
        [SerializeField] ObjsSet[] Sets;
        [SerializeField] ObjsById[] ByIds;
        readonly Disposes.All unsubscribe = new();
        void OnEnable()
        {
            foreach (ObjsSet topic in Sets)
            {
                topic.Collection.Add(gameObject);
                unsubscribe.Add(() => topic.Collection.Remove(gameObject));
            }
            foreach (ObjsById topic in ByIds)
            {
                topic.Collection.Add(gameObject.GetInstanceID(), gameObject);
                unsubscribe.Add(() => topic.Collection.Remove(gameObject.GetInstanceID()));
            }
        }
        void OnDisable() => unsubscribe.Dispose();
    }
}