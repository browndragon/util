using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Kinematic2DController")]
    public class Kinematic2DController : KinematicController<Rigidbody2D, Vector2>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            Vector2 velocity = InitialVelocity;
            if (IsRelative) velocity = transform.TransformVector(velocity);
            rigidbody.velocity = velocity;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}