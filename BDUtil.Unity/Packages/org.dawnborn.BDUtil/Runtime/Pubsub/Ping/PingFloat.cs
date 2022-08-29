using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/PingString")]
    [Tooltip("A ping whose parameter is a float")]
    public class PingFloat : ScriptableObject<Ping<float>> { }
}