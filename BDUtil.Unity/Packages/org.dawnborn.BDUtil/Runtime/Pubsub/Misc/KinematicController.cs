using System;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    public abstract class KinematicController : MonoBehaviour
    {
    }
    public abstract class KinematicController<TRB, TV> : KinematicController
    where TRB : Component
    where TV : struct
    {
        public Val<TV> Control;
        public TV GroundSpeed;
        public TV AirSpeed;
        public bool YIsAdd = true;
        [Tooltip("If true, the speeds are relative to my own facing")]
        public bool IsRelative = false;
        new protected TRB rigidbody;
        protected virtual void OnEnable() => rigidbody = GetComponent<TRB>();
        protected virtual void OnDisable() { }
    }
}