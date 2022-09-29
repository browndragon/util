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
        [SerializeField] protected UnityEvent Action = new();

        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField, Invoke(nameof(Publish))]
        Invoke.Button publish;
        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField, Invoke(nameof(DebugPrintSubscribers))]
        Invoke.Button debugPrintSubscribers;
        [Flags]
        public enum TraceOns
        {
            None = default,
            Add,
            Remove,
            Publish,
        };
        public TraceOns TraceOn = Enums<TraceOns>.Everything;

        public Lock IsPublishing { get; protected set; }
        public void AddListener(Action action)
        {
            if (TraceOn.HasFlag(TraceOns.Add)) Debug.Log($"{this}.Add({action.Target}.{action.Method})");
            Action.AddListener(Converter<Action, UnityAction>.Default.Convert(action));
        }
        public void RemoveListener(Action action)
        {
            if (TraceOn.HasFlag(TraceOns.Remove)) Debug.Log($"{this}.Remove({action.Target}.{action.Method})");
            Action.RemoveListener(Converter<Action, UnityAction>.Default.Convert(action));
        }
        public void RemoveAllListeners()
        {
            if (TraceOn.HasFlag(TraceOns.Remove)) Debug.Log($"{this}.RemoveAll()");
            Action.RemoveAllListeners(); IsPublishing = default;
        }
        public void Publish()
        {
            if (TraceOn.HasFlag(TraceOns.Publish)) Debug.Log($"{this}.Publish()");
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
        protected void DebugPrintSubscribers()
        {
            Debug.Log($"All subscribers {this}:---", this);
            int i = 0;
            foreach (Delegate subscriber in ReflectionUtils.GetSubscribers(Action))
            {
                Debug.Log($"#{i}: {subscriber?.Target}.{subscriber?.Method} => {subscriber?.GetInvocationList().Summarize()}", subscriber?.Target as UnityEngine.Object ?? this);
            }
            Debug.Log($"---:All subscribers {this}", this);
        }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() => RemoveAllListeners();
    }
}