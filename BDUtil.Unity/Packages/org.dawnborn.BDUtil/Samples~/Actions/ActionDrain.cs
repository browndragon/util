using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil
{
    public class ActionDrain : SingletonAsset<ActionDrain>
    {
        public Ticker.Event[] TickerEvents;
        public ActionsHead ActionHead;
        readonly Disposes.All unsubscribe = new();
        protected override void OnEnableSubsystem()
        {
            base.OnEnableSubsystem();
            unsubscribe.Add(ActionHead.Subscribe(a => a()));
            foreach (Ticker.Event @event in TickerEvents)
            {
                Debug.Log($"Subscribing {ActionHead}.Pop to {@event}");
                Ticker.main.Topics[@event].Subscribe(ActionHead.Pop);
            }
        }
        protected override void OnDisableSubsystem()
        {
            unsubscribe.Dispose();
            base.OnDisableSubsystem();
        }

    }
}