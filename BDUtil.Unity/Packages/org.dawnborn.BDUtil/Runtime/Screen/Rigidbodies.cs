using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class Rigidbodies
    {
        [Serializable]
        public struct Snapshot : Snapshots.ISnapshot<Snapshot>
        {
            public Vector3 Velocity;
            public Vector3 AngularVelocity;

            public Snapshot Lerp(in Snapshot b, float t) => new()
            {
                Velocity = Vector3.Lerp(Velocity, Velocity.Overridden(b.Velocity), t),
                AngularVelocity = Vector3.Lerp(AngularVelocity, AngularVelocity.Overridden(b.AngularVelocity), t)
            };

            // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
            public void Override(in Snapshot overrides)
            {
                Velocity.Override(overrides.Velocity);
                AngularVelocity.Override(overrides.AngularVelocity);
            }

            public override string ToString() => $"Rididbody({Velocity},{AngularVelocity})";
        }
        [Serializable]
        public struct Fuzz : Snapshots.ITarget<Snapshot>
        {
            public Transforms.Target Transform;
            public Randoms.Fuzzy<Vector3> VelocityFuzz;
            public Randoms.Fuzzy<Vector3> AngularVelocityFuzz;
            public Snapshot GetTarget(Snapshots.IFuzzControls controls, in Snapshot start)
            {
                Snapshot @return = start;
                @return.Velocity = controls.Random.Fuzzed(controls.Power * controls.Speed * VelocityFuzz.Pivot, VelocityFuzz.Fuzz, start.Velocity);
                @return.AngularVelocity = controls.Random.Fuzzed(controls.Power * controls.Speed * AngularVelocityFuzz.Pivot, AngularVelocityFuzz.Fuzz, start.AngularVelocity);
                return @return;
            }
        }

        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Snapshot GetLocalSnapshot(this Rigidbody2D thiz)
        => new()
        {
            Velocity = thiz.velocity,
            AngularVelocity = new(0f, 0f, thiz.angularVelocity),
        };
        /// Consider using GetLocalSnapshot to figure out which fields you _don't want to set_ first...
        public static void SetFromLocalSnapshot(this Rigidbody2D thiz, Snapshot target)
        {
            thiz.velocity = thiz.velocity.Overridden(target.Velocity);
            thiz.angularVelocity = float.IsFinite(target.AngularVelocity.z) ? target.AngularVelocity.z : thiz.angularVelocity;
        }

        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Snapshot GetLocalSnapshot(this Rigidbody thiz)
        => new()
        {
            Velocity = thiz.velocity,
            AngularVelocity = thiz.angularVelocity,
        };
        /// Consider using GetLocalSnapshot to figure out which fields you _don't want to set_ first...
        public static void SetFromLocalSnapshot(this Rigidbody thiz, Snapshot target)
        {
            thiz.velocity = thiz.velocity.Overridden(target.Velocity);
            thiz.angularVelocity = thiz.angularVelocity.Overridden(target.AngularVelocity);
        }
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
