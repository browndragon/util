using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/TickTopic")]
    public class TickTopic : Topic
    {
        [SerializeField] Lifecycle.Event tickerEvent;
        readonly Disposes.All unsubscribe = new();
        public Lifecycle.Event TickerEvent
        {
            get => tickerEvent;
            set
            {
                tickerEvent = value;
                OnValidate();
            }
        }
        void OnValidate()
        {
            if (!Application.isPlaying) return;
            unsubscribe.Dispose();
            if (Application.isPlaying) unsubscribe.Add(Lifecycle.main.Topics[tickerEvent].Subscribe(Publish));
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying) unsubscribe.Add(Lifecycle.main.Topics[tickerEvent].Subscribe(Publish));
        }
        protected override void OnDisable()
        {
            unsubscribe.Dispose();
            base.OnDisable();
        }
    }
}