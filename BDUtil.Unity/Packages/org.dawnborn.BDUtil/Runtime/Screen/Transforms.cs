using System;
using BDUtil.Fluent;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class Transforms
    {
        [Flags]
        public enum Overrides
        {
            None = 0,
            PosXYZ = PosX | PosY | PosZ,
            PosX = 1 << 0,
            PosY = 1 << 1,
            PosZ = 1 << 2,
            RotXYZ = RotX | RotY | RotZ,
            RotX = 1 << 3,
            RotY = 1 << 4,
            RotZ = 1 << 5,
            ScaleXYZ = ScaleX | ScaleY | ScaleZ,
            ScaleX = 1 << 6,
            ScaleY = 1 << 7,
            ScaleZ = 1 << 8,
        }
        [Serializable]
        public struct Local : Snapshots.ISnapshot<Local, Overrides>
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
            public Local(Vector3 position, Vector3 euler, Vector3 scale)
            {
                Position = position;
                EulerAngles = euler;
                scale0 = scale - Vector3.one;
            }
            public Local(Local other) : this(other.Position, other.EulerAngles, other.Scale) { }

            public Matrix4x4 AsMatrix => Matrix4x4.TRS(Position, Rotation, Scale);
            public static Local FromMatrix(in Matrix4x4 trs)
            => new()
            {
                Position = trs.GetPosition(),
                EulerAngles = trs.rotation.eulerAngles,
                Scale = trs.lossyScale
            };

            public Local Lerp(in Local b, float t) => new()
            {
                Position = Vector3.Lerp(Position, b.Position, t),
                EulerAngles = Vector3.Lerp(EulerAngles, b.EulerAngles, t),
                Scale = Vector3.Lerp(Scale, b.Scale, t)
            };
            // Makes this "local" more global by stacking it under the other one.
            public void Contextualize(in Local other)
            => this = FromMatrix(other.AsMatrix * AsMatrix);

            public void TermwiseSum(in Local other)
            {
                Position += other.Position;
                EulerAngles += other.EulerAngles;
                Scale += other.Scale;
            }

            // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
            public void Override(Overrides overrideField, in Local overrides)
            {
                Position = UpdateSnapshot(Position, overrides.Position,
                    overrideField.HasFlag(Overrides.PosX), overrideField.HasFlag(Overrides.PosY), overrideField.HasFlag(Overrides.PosZ));
                EulerAngles = UpdateSnapshot(EulerAngles, overrides.EulerAngles,
                    overrideField.HasFlag(Overrides.RotX), overrideField.HasFlag(Overrides.RotY), overrideField.HasFlag(Overrides.RotZ));
                Scale = UpdateSnapshot(Scale, overrides.Scale,
                    overrideField.HasFlag(Overrides.ScaleX), overrideField.HasFlag(Overrides.ScaleY), overrideField.HasFlag(Overrides.ScaleZ));
            }
            static Vector3 UpdateSnapshot(Vector3 @base, Vector3 @override, bool overrideX, bool overrideY, bool overrideZ)
            => new(
                overrideX ? @override.x : @base.x,
                overrideY ? @override.y : @base.y,
                overrideZ ? @override.z : @base.z
            );

            public override string ToString() => $"Local(Pos={Position},Rot={EulerAngles},Scale={Scale})";
        }
        [Serializable]
        public struct Fuzz : Snapshots.IFuzz<Local, Overrides>
        {
            // It coincidentally happens that every term is a float, and we're okay with just varying their float amount! Wild!
            public Vector3 PositionFuzz;
            public Vector3 EulerAnglesFuzz;
            public Vector3 Scale0Fuzz;
            float Apply(Randoms.UnitRandom random, bool hasFlag, float @base, float fuzz)
            => @base + (hasFlag ? random.Range(-fuzz, fuzz) : 0f);
            Vector3 Apply(Randoms.UnitRandom random, Overrides flags, Vector3 @base, Vector3 fuzz, Overrides x, Overrides y, Overrides z)
            => new(
                Apply(random, flags.HasFlag(x), @base.x, fuzz.x),
                Apply(random, flags.HasFlag(y), @base.y, fuzz.y),
                Apply(random, flags.HasFlag(z), @base.z, fuzz.z)
            );
            public void Apply(Snapshots.IFuzzControls controls, Overrides fuzzFields, in Local start, ref Local target)
            {
                // TODO: fix all the variables which are wrong
                target.Position = Apply(controls.Random, fuzzFields, target.Position, PositionFuzz, Overrides.PosX, Overrides.PosY, Overrides.PosZ);
                target.EulerAngles = Apply(controls.Random, fuzzFields, target.EulerAngles, EulerAnglesFuzz, Overrides.RotX, Overrides.RotY, Overrides.RotZ);
                target.Scale = Apply(controls.Random, fuzzFields, target.Scale, Scale0Fuzz, Overrides.ScaleX, Overrides.ScaleY, Overrides.ScaleZ);
            }
        }
        // Takes a snapshot, applying whatever overrides you specifying using a previous snapshot & a masking layer.
        public static Local GetLocalSnapshot(this Transform thiz)
        => new()
        {
            Position = thiz.localPosition,
            EulerAngles = thiz.localEulerAngles,
            Scale = thiz.localScale,
        };
        /// Consider using GetLocalSnapshot to figure out which fields you _don't want to set_ first...
        public static void SetFromLocalSnapshot(this Transform thiz, Local target)
        {
            thiz.localScale = target.Scale;
            thiz.localEulerAngles = target.EulerAngles;
            thiz.localPosition = target.Position;
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
