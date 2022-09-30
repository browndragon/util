using System;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    // MAYBE this belongs in Base, since it's actually the base class of the hierarchy. Or not!
    [CreateAssetMenu(menuName = "BDUtil/Prim/Topic", order = -1)]
    public class Topic : ScriptableObject, ITopic, IPublisher
    {
        [SerializeField, SuppressMessage("IDE", "IDE0051"), SuppressMessage("IDE", "IDE0044")]
        Invokable.Layout invokeButtons;
        [SerializeField] protected UnityEvent Action = new();
        public Lock IsPublishing { get; protected set; }
        public void AddListener(Action action)
        {
            TopicDebugging.main.LogOnAddListener(this, action);
            Action.AddListener(Converter<Action, UnityAction>.Default.Convert(action));
        }
        public void RemoveListener(Action action)
        {
            TopicDebugging.main.LogOnRemoveListener(this, action);
            Action.RemoveListener(Converter<Action, UnityAction>.Default.Convert(action));
        }
        [Invokable(order = +100)]
        public void RemoveAllListeners()
        {
            TopicDebugging.main.LogOnRemoveAllListeners(this);
            Action.RemoveAllListeners();
            IsPublishing = default;
        }
        [Invokable(order = -1)]
        public void Publish()
        {
            if (IsPublishing++)  // Increase the amount of publishing, forcing renotification.
            {
                Debug.Log($"Suppressing {this}.Publish(); already in flight.", this);
                return;
            }
            try
            {
                for (int i = 0; IsPublishing; ++i)
                {
                    IsPublishing = (Lock)1;  // Discard any additional isPublishing we had entering the loop
                    TopicDebugging.main.LogOnPublish(this);
                    Action?.Invoke();  // invoke the actions; some might republish: if they do, detect it:
                    if (i > 7) throw new NotSupportedException($"Too many ({i}) republishes on {this}");
                    if (--IsPublishing) Debug.Log($"Republishing ({i}th time) on {this}", this);
                }
            }
            finally { IsPublishing = false; }
        }
        [Invokable(order = +10)]
        protected void DebugPrintSubscribers()
        {
            Debug.Log($"All subscribers of {this}:---", this);
            int i = 0;
            foreach (Delegate subscriber in ReflectionUtils.GetSubscribers(Action))
            {
                Debug.Log($"#{i++}: target={subscriber?.Target}.method={subscriber?.Method}", subscriber?.Target as UnityEngine.Object ?? this);
            }
            Debug.Log($"---:All {i} subscribers of {this}", this);
        }
        protected virtual void OnEnable()
        {
            TopicDebugging.main.LogOnEnable(this);
        }
        protected virtual void OnDisable()
        {
            TopicDebugging.main.LogOnDisable(this);
            Action.RemoveAllListeners();
            IsPublishing = default;
        }
    }
}