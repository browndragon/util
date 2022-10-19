using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Kinematic2DController")]
    [RequireComponent(typeof(Groundling))]
    public class Kinematic2DController : KinematicController<Rigidbody2D, Vector2>
    {
        protected void Update()
        {
            Vector2 speed = Control.Value;
            float origJump = speed.y;
            speed.x *= GetWalkdXZ();
            speed.y = rigidbody.velocity.y;
            Vector2 _ = default;
            rigidbody.velocity = Vector2.SmoothDamp(rigidbody.velocity, speed, ref _, MovementSmoothing);
            if (speed.x * rigidbody.velocity.x < 0)
            {
                transform.localScale = transform.localScale.WithX(-transform.localScale.x);
            }
            if (origJump > 0)
            {
                float jumpfactor = GetJumpdY();
                if (jumpfactor >= 0) rigidbody.AddForce(jumpfactor * Vector2.up, ForceMode2D.Force);
            }
        }
    }
}