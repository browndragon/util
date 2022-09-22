using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/StringTopic")]
    [Bind.Impl(typeof(ValueTopic<string>))]
    public class StringTopic : ValueTopic<string>
    {
        [Serializable]
        public class UEventString : Subscribers.UEventValue<string> { }
    }
}