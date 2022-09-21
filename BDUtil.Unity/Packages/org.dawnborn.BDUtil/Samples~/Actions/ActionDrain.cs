using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil
{
    public class ActionDrain : SingletonAsset<ActionDrain>
    {
        public Ticker.Event[] TickerEvents;
        public ActionsHead ActionHead;
        public Topic[] Subscriptions;
        readonly Disposes.All unsubscribe = new();
        protected override void OnEnableSubsystem()
        {
            base.OnEnableSubsystem();
            unsubscribe.Add(ActionHead.Subscribe(a => a()));
            foreach (Ticker.Event @event in TickerEvents)
            {
                Debug.Log($"Subscribing {ActionHead}.Pop to {@event}");
                unsubscribe.Add(Ticker.main.Topics[@event].Subscribe(ActionHead.Pop));
            }
            foreach (Topic t in Subscriptions)
            {
                Debug.Log($"Subscribing to {t}");
                unsubscribe.Add(t.Subscribe(OnTopic));
            }
        }
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
        protected override void OnDisableSubsystem()
        {
            unsubscribe.Dispose();
            base.OnDisableSubsystem();
        }

    }
}