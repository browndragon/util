using System;
using BDUtil.Pubsub;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    [FilePath]
    public class ActionDrain : StaticAsset<ActionDrain>
    {
        public Lifecycle.Event[] TickerEvents;
        public ActionsHead ActionHead;
        public Topic[] Subscriptions;
        readonly Disposes.All unsubscribe = new();

        protected void OnEnable()
        {
            if (ActionHead == null) throw new ArgumentNullException(nameof(ActionHead), this.ToString());
            unsubscribe.Add(ActionHead.Subscribe(a => a()));

            /// scriptableobjects are loaded too early to actually use ticker.main, so guard that behind a slow load.
            Serialization.Subsystem.OnReady += () =>
            {
                foreach (Lifecycle.Event @event in TickerEvents) unsubscribe.Add(
                    Lifecycle.main.Topics[@event].Subscribe(ActionHead.Pop)
                );
            };

            foreach (Topic t in Subscriptions) unsubscribe.Add(
                t.Subscribe(OnTopic)
            );
        }
        protected void OnDisable() => unsubscribe.Dispose();
        protected void OnTopic(ITopic t)
        {
            switch (t)
            {
                case ObjectTopic o:
                    Debug.Log($"Got update {t}: {o.Object}");
                    break;
                default:
                    Debug.Log($"Got update {t}");
                    break;
            }
        }
    }
}