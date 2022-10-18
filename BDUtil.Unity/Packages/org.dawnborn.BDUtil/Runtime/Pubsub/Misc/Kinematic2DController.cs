using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Kinematic2DController")]
    [RequireComponent(typeof(Groundling))]
    public class Kinematic2DController : KinematicController<Rigidbody2D, Vector2>
    {
        Kinematic2DController()
        {
            GroundSpeed = new(6f, 120f);
            AirSpeed = new(2f, 0f);
        }
        Groundling groundling;
        readonly Disposes.All unsubscribe = new();
        protected override void OnEnable()
        {
            base.OnEnable();
            rigidbody.velocity = Control.Value;
            groundling = GetComponent<Groundling>();
        }
        protected override void OnDisable()
        {
            unsubscribe.Dispose();
            base.OnDisable();
        }
        protected void Update()
        {
            Vector2 speed = GroundSpeed;
            if (!groundling.OnGround.Value)
            {  // Falling or jumping, after some slight grace frames
                speed = AirSpeed;
            }
            speed = Vector2.Scale(speed, Control.Value);
            Vector2 veloc = rigidbody.velocity;
            veloc.x = speed.x;
            if (YIsAdd) veloc.y += Time.deltaTime * speed.y;
            else veloc.y = speed.y;
            rigidbody.velocity = veloc;
        }
    }
}