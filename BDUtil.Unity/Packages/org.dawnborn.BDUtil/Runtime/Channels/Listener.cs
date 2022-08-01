using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Channels
{
    [AddComponentMenu("BDUtil/Listener")]
    [Tooltip("Binds listeners<->Channels<->UnityEvent(s). You may find it easier to implement directly in your class(es).")]
    public class Listener : MonoBehaviour
    {
        // By convention, the UnityEvents are on this same gameObject.
        [Serializable]
        public struct Binding
        {
            public Channel[] Channels;
            public UnityEvent OnChannel;
        }
        public Binding[] Bindings;
        /// Logging or debugging;
        public UnityEvent<Channel> OnAnyChannel = new();
        /// For use OnAnyChannel.
        void DebugLog(Channel c) => Debug.Log($"Channel {c} received message", this);

        Disposes.All Dispose = new();

        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable()
        {
            Dispose?.Dispose();
            foreach (Binding binding in Bindings ?? Array.Empty<Binding>())
            {
                foreach (Channel channel in binding.Channels ?? Array.Empty<Channel>())
                {
                    if (binding.OnChannel != null) channel.Subscribe(binding.OnChannel, Dispose);
                    if (OnAnyChannel.GetPersistentEventCount() > 0) channel.Subscribe(() => OnAnyChannel.Invoke(channel), Dispose);
                }
            }
        }
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable()
        {
            Dispose?.Dispose();
        }
    }
}