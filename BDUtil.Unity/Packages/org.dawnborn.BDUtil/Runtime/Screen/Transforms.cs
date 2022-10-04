using System;
using BDUtil.Fluent;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class Transforms
    {
        [Flags]
        public enum Masks
        {
            None = 0,
            PosX = 1 << 0,
            PosY = 1 << 1,
            PosZ = 1 << 2,
            RotX = 1 << 3,
            RotY = 1 << 4,
            RotZ = 1 << 5,
            ScaleX = 1 << 6,
            ScaleY = 1 << 7,
            ScaleZ = 1 << 8,
        }
        [Serializable]
        public struct Local
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
            Vector3 scale0;
            public Vector3 Scale
            {
                get => Vector3.one + scale0;
                set => scale0 = value - Vector3.one;
            }
            public Matrix4x4 AsMatrix => Matrix4x4.TRS(Position, Rotation, Scale);
            public static Local FromMatrix(in Matrix4x4 trs)
            => new()
            {
                Position = trs.GetPosition(),
                EulerAngles = trs.rotation.eulerAngles,
                Scale = trs.lossyScale
            };

            public static Local Lerp(Local a, Local b, float t) => new()
            {
                Position = Vector3.Lerp(a.Position, b.Position, t),
                EulerAngles = Vector3.Lerp(a.EulerAngles, b.EulerAngles, t),
                Scale = Vector3.Lerp(a.EulerAngles, b.EulerAngles, t)
            };
            // Makes this "local" more global by stacking it under the other one.
            public void AdjustBy(in Local other)
            => this = FromMatrix(other.AsMatrix * AsMatrix);
        }
        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Local GetLocalSnapshot(this Transform thiz, Masks masks = default, Local snapshot = default)
        => new()
        {
            Position = UpdateSnapshot(thiz.localPosition, snapshot.Position,
                masks.HasFlag(Masks.PosX), masks.HasFlag(Masks.PosY), masks.HasFlag(Masks.PosZ)),
            EulerAngles = UpdateSnapshot(thiz.localEulerAngles, snapshot.EulerAngles,
                masks.HasFlag(Masks.RotX), masks.HasFlag(Masks.RotY), masks.HasFlag(Masks.RotZ)),
            Scale = UpdateSnapshot(thiz.localScale, snapshot.Scale,
                masks.HasFlag(Masks.ScaleX), masks.HasFlag(Masks.ScaleY), masks.HasFlag(Masks.ScaleZ)),
        };
        static Vector3 UpdateSnapshot(Vector3 @base, Vector3 @override, bool overrideX, bool overrideY, bool overrideZ)
        => new(
            overrideX ? @override.x : @base.x,
            overrideY ? @override.y : @base.y,
            overrideZ ? @override.z : @base.z
        );
        /// Consider using GetLocalSnapshot to figure out which fields you _don't want to set_ first...
        public static void SetFromLocalSnapshot(this Transform thiz, Local target)
        {
            thiz.localScale = target.Scale;
            thiz.localEulerAngles = target.EulerAngles;
            thiz.localPosition = target.Position;
        }
        public struct IndexTransform : Lists.IMicroList<Transform>
        {
            readonly Transform Parent;
            public IndexTransform(Transform parent) => Parent = parent;
            public int Count => Parent.childCount;
            public Transform this[int i] => Parent.GetChild(i);
        }

        public static Lists.Enumerator<IndexTransform, Transform> GetChildren(this Transform thiz)
        => new(new IndexTransform(thiz));
    }
}
