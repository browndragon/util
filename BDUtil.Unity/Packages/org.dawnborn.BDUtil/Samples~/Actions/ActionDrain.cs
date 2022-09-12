using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil
{
    public class ActionDrain : SingletonAsset<ActionDrain>
    {
        public Ticker.Event[] TickerEvents;
        public ActionsHead ActionHead;
        readonly Disposes.All unsubscribe = new();
        bool needsReenable;
        protected override void OnRuntimeInitialize(RuntimeInitializeLoadType type)
        {
            if (type != RuntimeInitializeLoadType.AfterSceneLoad) return;
            needsReenable = false;
            Begin();
        }
        protected override void OnDisable()
        { unsubscribe.Dispose(); base.OnDisable(); needsReenable = Application.isPlaying; }
        protected override void OnEnable()
        { base.OnEnable(); if (needsReenable) Begin(); }

        // I don't think there's any way to suspend/resume a scriptableobject per se BUT:
        // This is called from OnEnable (which can be called outside of app.isPlaying...)
        // and from the runtime initialize path.
        void Begin()
        {
            unsubscribe.Add(ActionHead.Subscribe(a => a()));
            foreach (Ticker.Event @event in TickerEvents)
            {
                Debug.Log($"Subscribing {ActionHead}.Pop to {@event}");
                Ticker.main.Topics[@event].Subscribe(ActionHead.Pop);
            }
        }
    }
}