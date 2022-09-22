using System;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Prim/V2ITopic")]
    [Bind.Impl(typeof(ValueTopic<Vector2Int>))]
    public class V2ITopic : ValueTopic<Vector2Int>
    {
        [Serializable]
        public class UEventV2I : Subscribers.UEventValue<Vector2Int> { }
    }
}