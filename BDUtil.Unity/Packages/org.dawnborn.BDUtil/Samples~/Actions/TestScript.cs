using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    public class TestScript : MonoBehaviour
    {
        public ActionsHead ActionQueue;
        public Timer Delay = .25f;

        protected void OnTriggerStay2D(Collider2D collider)
        {
            if (Delay) return;
            ActionQueue.Push(() => Debug.Log($"{collider} hit (via action queue!)"));
            Delay = Delay.Restart();
        }
    }
}