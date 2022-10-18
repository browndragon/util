using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    // Provide information about yes-or-no-on-ground.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Groundling : MonoBehaviour
    {
        public float GroundDistance = 1 / 128f;
        public bool RawOnGround;
        public Val<bool> OnGround;
        public Timer HangTime = .125f;
        public Timer LandTime = 0f;

        new Rigidbody2D rigidbody;

        protected void Awake() => rigidbody = GetComponent<Rigidbody2D>();
        protected void FixedUpdate()
        {
            scratch[0] = default;
            RawOnGround = rigidbody.Cast(Vector2.down, scratch, GroundDistance) > 0;
            scratch[0] = default;
        }

        protected void Update()
        {
            if (RawOnGround)
            {
                if (!LandTime.IsStarted) LandTime.Reset();
                if (OnGround.Value = LandTime.Tick.IsLive) HangTime.Stopped();
                return;
            }
            if (!HangTime.IsStarted) HangTime.Reset();
            if (OnGround.Value = HangTime.Tick.IsLive) LandTime.Stopped();
        }
        static readonly RaycastHit2D[] scratch = new RaycastHit2D[1];
    }
}