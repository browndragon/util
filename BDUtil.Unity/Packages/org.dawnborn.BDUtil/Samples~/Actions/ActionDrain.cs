using System;
using BDUtil.Pubsub;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    [FilePath]
    public class ActionDrain : StaticAsset<ActionDrain>
    {
        public Ticker.Event[] TickerEvents;
        public ActionsHead ActionHead;
        public Topic[] Subscriptions;
        readonly Disposes.All unsubscribe = new();

        void OnEnable()
        {
            if (ActionHead == null) throw new ArgumentNullException(nameof(ActionHead), this.ToString());
            unsubscribe.Add(ActionHead.Subscribe(a =>
            {
                Debug.Log($"Got action {a}");
                a();
            }));

            /// scriptableobjects are loaded too early to actually use ticker.main, so guard that behind a slow load.
            Ticker.OnMain += () =>
            {
                Debug.Log($"Ticker showed up; registering for ticks");
                foreach (Ticker.Event @event in TickerEvents)
                {
                    Debug.Log($"Subscribing {ActionHead}.Pop to {@event}");
                    unsubscribe.Add(Ticker.main.Topics[@event].Subscribe(ActionHead.Pop));
                }
            };

            foreach (Topic t in Subscriptions)
            {
                Debug.Log($"Subscribing to {t}");
                unsubscribe.Add(t.Subscribe(OnTopic));
            }
        }
        void OnDisable() => unsubscribe.Dispose();
        void OnTopic(ITopic t)
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