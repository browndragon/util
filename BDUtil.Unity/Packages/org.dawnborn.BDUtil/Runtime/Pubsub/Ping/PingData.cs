using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/PingData")]
    [Tooltip("A ping whose parameter is a json-like `IDict<string, object>`")]
    public class PingData : ScriptableObject<Ping<IDictionary<string, object>>> { }
}