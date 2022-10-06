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
        public bool IsRelative;
        public TV InitialVelocity;
        new protected TRB rigidbody;
        protected virtual void OnEnable()
        {
            rigidbody = GetComponent<TRB>();
        }
        protected virtual void OnDisable()
        {
        }
    }
}