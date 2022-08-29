using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/PingGameObject")]
    [Tooltip("A ping whose parameter is a game object")]
    public class PingGameObject : ScriptableObject<Ping<GameObject>> { }
}