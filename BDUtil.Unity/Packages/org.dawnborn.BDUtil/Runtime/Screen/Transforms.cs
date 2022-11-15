using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class Transforms
    {
        [Serializable]
        public struct Snapshot : ISnapshot<Transform>
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
            public Snapshot(Transform transform) : this(transform.position, transform.eulerAngles, transform.localScale) { }
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
            public override string ToString() => $"Local(Pos={Position},Rot={EulerAngles},Scale={Scale})";

            public void ReadFrom(Transform transform) => this = new(transform);

            public void ApplyTo(Transform transform)
            {
                transform.localScale = Scale;
                transform.SetPositionAndRotation(Position, Quaternion.Euler(EulerAngles));
            }
            public void ReadFrom(GameObject player) => ReadFrom(player.transform);
            public void ApplyTo(GameObject player) => ApplyTo(player.transform);
        }
        [Serializable]
        public struct Target
        {
            // It coincidentally happens that every term is a float, and we're okay with just varying their float amount! Wild!
            [Tooltip("Adds to current localPosition (or zero if no parents)")]
            public Bounds Position;
            [Tooltip("Rotates current rotation.")]
            public Bounds EulerAngles;
            [Tooltip("Adds to current scale.")]
            public Bounds Scale0;
            public Target(Bounds position, Bounds eulerAngles, Bounds scale0)
            {
                Position = position;
                EulerAngles = eulerAngles;
                Scale0 = scale0;
            }
        }
        public static Snapshot Range(this Randoms.IRandom thiz, Target target) => new(
            thiz.Range(target.Position),
            thiz.Range(target.EulerAngles),
            Vector3.one + thiz.Range(target.Scale0)
        );

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
