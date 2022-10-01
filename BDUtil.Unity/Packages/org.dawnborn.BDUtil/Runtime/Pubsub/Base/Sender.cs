using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    /// Wraps send message, broadcast, etc. Serializably.
    /// This isn't _really_ pubsub per se, but it's *unity's* old *pub* mechanism...
    /// ... and I need it for subscriber to delegate to sending.
    [Serializable]
    public struct Sender
    {
        public string Name;
        public enum Targets
        {
            Send = default,
            SendUp,
            Broadcast
        }
        public Targets Target;
        public bool Require;
        public bool StripArgument;
        public SendMessageOptions RequireReceiver => Require ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver;
        public void Send(GameObject @object, object param = default)
        {
            if (StripArgument) param = null;
            switch (Target)
            {
                case Targets.Send: @object.SendMessage(Name, param, RequireReceiver); return;
                case Targets.SendUp: @object.SendMessageUpwards(Name, param, RequireReceiver); return;
                case Targets.Broadcast: @object.BroadcastMessage(Name, param, RequireReceiver); return;
                default: throw Target.BadValue();
            }
        }
        public void Send(Component c, object param = default)
        => Send(c?.gameObject, param);
    }
}
