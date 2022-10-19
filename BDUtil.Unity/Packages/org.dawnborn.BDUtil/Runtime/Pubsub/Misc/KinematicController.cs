using System;
using BDUtil.Math;
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
        public float GroundSpeed = 6f;
        public AnimationCurves.Scaled Jump = AnimationCurves.Interpolated0101(Easings.OutQuad).FlippedX();
        Timer JumpCooldown;

        [Range(0f, 1f)]
        public float MovementSmoothing = .05f;
        [Range(0f, 1f)]
        public float AirControl = .3333f;

        new protected TRB rigidbody;
        protected Groundling groundling;
        protected readonly Disposes.All unsubscribe = new();

        protected virtual void OnEnable()
        {
            rigidbody = GetComponent<TRB>();
            groundling = GetComponent<Groundling>();
            unsubscribe.Add(groundling.OnGround.Topic.Subscribe(Land));
        }
        protected virtual void OnDisable()
        {
            unsubscribe.Dispose();
        }
        protected virtual void Land(bool onGround)
        {
            if (!onGround) return;
            JumpCooldown = 0f;  // So we can jump again.
        }
        // Returns the Yspeed of the jump at current phase.
        public float GetJumpdY()
        {
            if (groundling.OnGround.Value)
            {
                if (!JumpCooldown.IsStarted || !JumpCooldown.Tick.IsLive) JumpCooldown = new(Jump.Bounds.width);
            }
            Tick tick = JumpCooldown.Tick;
            if (!tick.IsLive)
            {  // We're after the cooldown, so we can't jump (/again).
                return 0f;
            }
            return Jump.Evaluate(JumpCooldown.Tick.Passed);
        }
        // Returns the absval of XZ adjustment based on current regime;
        // during jump acceleration, we continue to allow hmove.
        public float GetWalkdXZ()
        => GroundSpeed * (groundling.OnGround.Value || JumpCooldown.Tick.IsLive ? 1f : AirControl);
    }
}