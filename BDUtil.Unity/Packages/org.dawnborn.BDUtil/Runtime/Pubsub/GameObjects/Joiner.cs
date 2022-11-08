using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Joiner")]
    [Tooltip("OnEnable joins all SetTopics as a member (!not! subscriber); OnDisable, leaves them.")]
    public class Joiner : MonoBehaviour
    {
        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField] ObjsSet[] Sets;
        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField] ObjsById[] ByIds;
        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField] GameObjectTopic[] GameObjects;
        readonly Disposes.All unsubscribe = new();
        [SuppressMessage("IDE", "IDE0051")]
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
            foreach (GameObjectTopic topic in GameObjects)
            {
                topic.Value = gameObject;
                unsubscribe.Add(() => { if (topic.Value == gameObject) topic.Value = null; });
            }
        }
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() => unsubscribe.Dispose();
    }
}