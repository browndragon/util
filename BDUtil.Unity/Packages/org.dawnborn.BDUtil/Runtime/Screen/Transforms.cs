using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;
using static BDUtil.Math.Randoms;

namespace BDUtil.Screen
{
    public static class Transforms
    {
        [Serializable]
        public struct Snapshot : Snapshots.ISnapshot<Snapshot>
        {
            public Vector3 Position;
            public Vector3 EulerAngles;
            public Quaternion Rotation
            {
                get => Quaternion.Euler(EulerAngles);
                set => EulerAngles = value.eulerAngles;
            }
            [Tooltip("To allow default etc to work: the scale-1, so that identity is 0.")]
            [SerializeField]
            internal Vector3 scale0;
            public Vector3 Scale
            {
                get => Vector3.one + scale0;
                set => scale0 = value - Vector3.one;
            }
            public Snapshot(Vector3 position, Vector3 euler, Vector3 scale)
            {
                Position = position;
                EulerAngles = euler;
                scale0 = scale - Vector3.one;
            }
            public Snapshot(Matrix4x4 matrix)
            {
                Position = matrix.GetPosition();
                EulerAngles = matrix.rotation.eulerAngles;
                scale0 = matrix.lossyScale - Vector3.one;
            }
            public Matrix4x4 Matrix => Matrix4x4.TRS(Position, Rotation, Scale);
            public static implicit operator Matrix4x4(Snapshot s) => s.Matrix;
            public static implicit operator Snapshot(Matrix4x4 m) => new(m);
            public Snapshot(Snapshot other) : this(other.Position, other.EulerAngles, other.Scale) { }
            public Snapshot Lerp(in Snapshot b, float t) => new()
            {
                Position = Vector3.Lerp(Position, b.Position, t),
                EulerAngles = Vector3.Lerp(EulerAngles, b.EulerAngles, t),
                Scale = Vector3.Lerp(Scale, b.Scale, t)
            };

            // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
            public void Override(in Snapshot overrides)
            {
                Position.Override(overrides.Position);
                EulerAngles.Override(overrides.EulerAngles);
                scale0.Override(overrides.scale0);
            }
            public override string ToString() => $"Local(Pos={Position},Rot={EulerAngles},Scale={Scale})";
        }
        [Serializable]
        public struct Target : Snapshots.ITarget<Snapshot>
        {
            // It coincidentally happens that every term is a float, and we're okay with just varying their float amount! Wild!
            [Tooltip("Adds to current localPosition (or zero if no parents)")]
            public Fuzzy<Vector3> Position;
            [Tooltip("Rotates current rotation.")]
            public Fuzzy<Vector3> EulerAngles;
            [Tooltip("Adds to current scale.")]
            public Fuzzy<Vector3> Scale0;
            public Target(Fuzzy<Vector3> position, Fuzzy<Vector3> eulerAngles, Fuzzy<Vector3> scale0)
            {
                Position = position;
                EulerAngles = eulerAngles;
                Scale0 = scale0;
            }
            public Snapshot GetTarget(Snapshots.IFuzzControls controls, in Snapshot start)
            => new Snapshot(controls.Random.Fuzzed(Position), controls.Random.Fuzzed(EulerAngles), Vector3.one + controls.Random.Fuzzed(Scale0)).Matrix * start.Matrix;
        }
        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Snapshot GetLocalSnapshot(this Transform thiz)
        => new()
        {
            Position = thiz.localPosition,
            EulerAngles = thiz.localEulerAngles,
            Scale = thiz.localScale,
        };
        /// Consider using GetLocalSnapshot to figure out which fields you _don't want to set_ first...
        public static void SetFromLocalSnapshot(this Transform thiz, Snapshot target)
        {
            thiz.localScale = thiz.localScale.Overridden(target.Scale);
            thiz.localEulerAngles = thiz.localEulerAngles.Overridden(target.EulerAngles);
            thiz.localPosition = thiz.localPosition.Overridden(target.Position);
        }
        public static void AddFromLocalSnapshot(this Transform thiz, Snapshot add)
        {
            thiz.localScale += add.scale0;
            thiz.localEulerAngles += add.EulerAngles;
            thiz.localPosition += add.Position;
        }
        public struct TransformChildren : Lists.IMicroList<Transform>
        {
            readonly Transform Parent;
            public TransformChildren(Transform parent) => Parent = parent;
            public int Count => Parent.childCount;
            public Transform this[int i] => Parent.GetChild(i);
        }

        public static Lists.Enumerator<TransformChildren, Transform> GetChildren(this Transform thiz)
        => new(new TransformChildren(thiz));
    }
}
