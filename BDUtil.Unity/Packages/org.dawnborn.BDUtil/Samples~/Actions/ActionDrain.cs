using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil
{
    public class ActionDrain : SingletonAsset<ActionDrain>
    {
        public Ticker.Event[] TickerEvents;
        public ActionsHead ActionHead;
        readonly Disposes.All unsubscribe = new();
        bool Begun { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BeginMain() => main.Begin(true);
        void Begin(bool isBeginMain)
        {
            Debug.Log($"Starting static drain {this}:{isBeginMain}", this);
            if (!(Begun |= isBeginMain)) return;
            unsubscribe.Add(ActionHead.Subscribe(a => a()));
            foreach (Ticker.Event @event in TickerEvents)
            {
                Debug.Log($"Subscribing {ActionHead}.Pop to {@event}");
                Ticker.main.Topics[@event].Subscribe(ActionHead.Pop);
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            Begin(false);
        }
        protected override void OnDisable()
        {
            unsubscribe.Dispose();
            base.OnDisable();
        }
    }
}