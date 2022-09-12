using System;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/Topic", order = 0)]
    public class Topic : ScriptableObject, ITopic, IPublisher
    {
        protected event Action Action;
        [field: SerializeField, OnChange(nameof(Publish), AsButton = true)]
        public Lock IsPublishing { get; protected set; }
        public void AddListener(Action action) => Action += action;
        public void RemoveListener(Action action) => Action -= action;
        public void ClearAll() { Action = Action.UnsubscribeAll(); IsPublishing = default; }
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
        protected virtual void OnEnable() => ClearAll();
        protected virtual void OnDisable() => ClearAll();
    }
}