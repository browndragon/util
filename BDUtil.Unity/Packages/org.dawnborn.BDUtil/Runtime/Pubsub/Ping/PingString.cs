using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/PingString")]
    [Tooltip("A ping whose parameter is a string")]
    public class PingString : ScriptableObject<Ping<string>> { }
}