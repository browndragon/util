using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class Rigidbodies
    {
        [Serializable]
        public struct Snapshot : ISnapshot<Rigidbody2D>, ISnapshot<Rigidbody>
        {
            public Vector3 Velocity;
            public Vector3 AngularVelocity;

            public Snapshot(Vector3 velocity, Vector3 angularVelocity)
            {
                Velocity = velocity;
                AngularVelocity = angularVelocity;
            }
            public Snapshot(Vector2 velocity, float angularVelocity) : this(velocity, angularVelocity * Vector3.forward) { }
            public Snapshot(Rigidbody2D rb2d) : this(rb2d.velocity, rb2d.angularVelocity) { }
            public Snapshot(Rigidbody rb) : this(rb.velocity, rb.angularVelocity) { }

            public void ReadFrom(Rigidbody2D player)
            => this = new(player);
            public void ReadFrom(Rigidbody player)
            => this = new(player);
            public void ReadFrom(GameObject player)
            {
                Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    ReadFrom(rb2d);
                    return;
                }
                ReadFrom(player.GetComponent<Rigidbody>());
            }

            public void ApplyTo(Rigidbody2D player)
            {
                player.velocity = Velocity;
                player.rotation = AngularVelocity.z;
            }

            public void ApplyTo(Rigidbody player)
            {
                player.velocity = Velocity;
                player.rotation = Quaternion.Euler(AngularVelocity);
            }

            public void ApplyTo(GameObject player)
            {
                Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    ApplyTo(rb2d);
                    return;
                }
                ApplyTo(player.GetComponent<Rigidbody>());
            }

            public override string ToString() => $"Rididbody({Velocity},{AngularVelocity})";
        }
        [Serializable]
        public struct Target
        {
            public Transforms.Target Transform;
            public Bounds Velocity;
            public Bounds AngularVelocity;
        }
        public static Snapshot GetTarget(this Randoms.IRandom thiz, Target target) => new(
            thiz.Range(target.Velocity),
            thiz.Range(target.AngularVelocity)
        );

        public static void Bounce(this Rigidbody2D thiz, Bounds bounds)
        {
            Vector3 velocity = thiz.velocity;
            bounds.Bounce(thiz.position, ref velocity);
            thiz.velocity = velocity;
        }
        public static void Bounce(this Rigidbody thiz, Bounds bounds)
        {
            Vector3 velocity = thiz.velocity;
            bounds.Bounce(thiz.position, ref velocity);
            thiz.velocity = velocity;
        }
    }
}
