using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/StringTopic")]
    public class StringTopic : ValueTopic<string>
    {
        [Serializable]
        public class UEventString : Subscribers.UEventValue<string> { }
    }
}