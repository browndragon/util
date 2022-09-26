using System;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    // MAYBE this belongs in Base, since it's actually the base class of the hierarchy. Or not!
    [CreateAssetMenu(menuName = "BDUtil/Prim/Topic", order = 0)]
    public class Topic : ScriptableObject, ITopic, IPublisher
    {
        [SerializeField] protected UnityEvent Action;

        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField, Invoke(nameof(Publish))]
        Invoke.Button publish;

        public Lock IsPublishing { get; protected set; }
        public void AddListener(Action action) => Action.AddListener(Converter<Action, UnityAction>.Default.Convert(action));
        public void RemoveListener(Action action) => Action.RemoveListener(Converter<Action, UnityAction>.Default.Convert(action));
        public void RemoveAllListeners() { Action.RemoveAllListeners(); IsPublishing = default; }
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
                    Action?.Invoke();  // invoke the actions; some might republish: if they do, detect it:
                    if (i > 7) throw new NotSupportedException($"Too many ({i}) republishes on {this}");
                    if (--IsPublishing) Debug.Log($"Republishing ({i}th time) on {this}", this);
                }
            }
            finally { IsPublishing = false; }
        }
        protected virtual void OnEnable() => RemoveAllListeners();
        protected virtual void OnDisable() => RemoveAllListeners();
    }
}