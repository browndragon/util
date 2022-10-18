using System.Collections;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    // Provide information about yes-or-no-on-ground.
    [AddComponentMenu("BDUtil/Groundling")]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Groundling : MonoBehaviour
    {
        public float GroundDistance = 1 / 128f;
        bool needsInitialization;
        public bool RawOnGround;
        public Val<bool> OnGround;
        public Timer HangTime = .125f;
        public Timer LandTime = 0f;
        // For instance, if jumping, it's [now-(time jumped + hang time)].
        public Timer TimeInState = default;

        new Rigidbody2D rigidbody;

        protected void Awake() => rigidbody = GetComponent<Rigidbody2D>();
        protected void OnEnable()
        {
            needsInitialization = true;
        }
        protected void OnDisable() { }

        protected void FixedUpdate()
        {
            scratch[0] = default;
            RawOnGround = rigidbody.Cast(Vector2.down, filter, scratch, GroundDistance) > 0;
            if (RawOnGround) Debug.Log($"OnGround: {scratch[0].collider.IDStr()}", scratch[0].collider);
            scratch[0] = default;
        }
        static readonly ContactFilter2D filter = new()
        {
            useTriggers = false,
        };

        protected void Update()
        {
            if (needsInitialization)
            {
                OnGround.Value = RawOnGround;
                TimeInState.Reset();
                return;
            }
            if (!(RawOnGround ^ OnGround.Value)) return;
            if (RawOnGround)
            {
                if (!LandTime.IsStarted) LandTime.Reset();
                if (LandTime.Tick.IsLive) return;
                OnGround.Value = true;
                TimeInState.Reset();
                HangTime.Stopped();
                return;
            }
            if (!HangTime.IsStarted) HangTime.Reset();
            if (HangTime.Tick.IsLive) return;
            OnGround.Value = false;
            TimeInState.Reset();
            LandTime.Stopped();
        }
        static readonly RaycastHit2D[] scratch = new RaycastHit2D[1];
    }
}