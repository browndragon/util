using System;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Play
{
    public static class Metaphysics
    {
        public static readonly Collider2D[] ScratchColliders = new Collider2D[32];
        public static readonly RaycastHit2D[] ScratchHits = new RaycastHit2D[32];
        public enum ShapeTypes
        {
            Point = default,
            Circle,
            Area,
            Box,
        }
        [Serializable]
        public struct Shape
        {
            public ShapeTypes ShapeType;
            /// Slightly abusive, since this is interpreted as a scale.
            /// Points use no fields.
            /// Circles use x.
            /// Areas use x, y (and if rot%180 in [45,135], switch them)
            /// Rects use x, y, rot.
            public Dimension Dimension;
        }
        [Serializable]
        public struct Dimension
        {
            [Tooltip("X & Y are size; Z is rotation degrees about Z axis")]
            public Vector3 value;
            public Vector2 Size { get => value; set => this.value = value.AsXYZ(this.value.z); }
            public Vector2 Extents => Size / 2;
            public float Width => Size.x;
            public float Height => Size.y;
            public float Rotation { get => value.z; set => this.value = this.value.WithZ(value); }
            public Dimension(Vector3 value) => this.value = value;
            public Dimension(Vector2 scale, float rotation = 0f) : this(scale.AsXYZ(rotation)) { }
            public static implicit operator Dimension(Vector2 scale) => new(scale);
            public static Dimension operator +(in Dimension a, in Dimension b) => new(a.value + b.value);
            public static Dimension operator -(in Dimension a, in Dimension b) => new(a.value - b.value);
            public static Dimension operator -(in Dimension a) => new(a.value);
        }
        [Serializable]
        public struct Direction
        {
            [Tooltip("X is a rotation angle, and Y is a distance in that angle.")]
            public Vector2 value;
            public float Angle { get => value.x; set => this.value.x = value; }
            public Vector2 Vector2 { get => Vectors.OfAngle(value.x); set => this.value = value.WithX(Vector2.SignedAngle(Vector2.right, value)); }
            public float Distance { get => value.y; set => this.value = this.value.WithY(value); }
            public Direction(float angle, float distance = float.PositiveInfinity) => value = new(angle, distance);
            public Direction(Vector2 normal, float distance = float.PositiveInfinity) : this(Vector2.SignedAngle(Vector2.right, normal), distance) { }
            public static implicit operator Direction(Vector2 normal) => new(normal);
            public static implicit operator Direction(float angle) => new(angle);
        }
        [Serializable]
        public struct QueryParams
        {
            public bool IsValid;
            public LayerMask layerMask;
            public float minDepth;
            public float maxDepth;
            public QueryParams(LayerMask layerMask, float minDepth, float maxDepth)
            {
                IsValid = true;
                this.layerMask = layerMask;
                this.minDepth = minDepth;
                this.maxDepth = maxDepth;
            }
            public LayerMask LayerMask => IsValid ? layerMask : Physics2D.DefaultRaycastLayers;
            public float MinDepth => IsValid ? minDepth : float.NegativeInfinity;
            public float MaxDepth => IsValid ? maxDepth : float.PositiveInfinity;
        }
        [Serializable]
        public struct Query
        {
            public static readonly Query Default = new()
            {
                Cast = 0f,
            };
            public Shape Shape;
            public Direction Cast;
            public QueryParams QueryParams;

            public Collider2D GetOverlap(Dimension offset)
            {
                Vector2 position = offset.Size;
                return Shape.ShapeType switch
                {
                    ShapeTypes.Point => Physics2D.OverlapPoint(position, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Circle => Physics2D.OverlapCircle(position, Shape.Dimension.Width, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Area => Physics2D.OverlapArea(position - Shape.Dimension.Extents, position + Shape.Dimension.Extents, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Box => Physics2D.OverlapBox(position, Shape.Dimension.Size, offset.Rotation + Shape.Dimension.Rotation, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    _ => throw Shape.ShapeType.BadValue(),
                };
            }

            public RaycastHit2D GetCast(Dimension offset)
            {
                Vector2 position = offset.Size;
                Direction cast = Cast;
                cast.Angle += offset.Rotation;
                Vector2 castDirection = cast.Vector2;
                return Shape.ShapeType switch
                {
                    ShapeTypes.Point => Physics2D.Raycast(position, castDirection, Cast.Distance, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Circle => Physics2D.CircleCast(position, Shape.Dimension.Width, castDirection, Cast.Distance, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Area => Physics2D.BoxCast(position - Shape.Dimension.Extents, position + Shape.Dimension.Extents, 0f, castDirection, Cast.Distance, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Box => Physics2D.BoxCast(position, Shape.Dimension.Size, offset.Rotation + Shape.Dimension.Rotation, castDirection, Cast.Distance, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    _ => throw Shape.ShapeType.BadValue(),
                };
            }

            public int GetOverlapAll(Dimension offset, Collider2D[] hits)
            {
                Vector2 position = offset.Size;
                return Shape.ShapeType switch
                {
                    ShapeTypes.Point => Physics2D.OverlapPointNonAlloc(position, hits, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Circle => Physics2D.OverlapCircleNonAlloc(position, Shape.Dimension.Width, hits, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Area => Physics2D.OverlapAreaNonAlloc(position - Shape.Dimension.Extents, position + Shape.Dimension.Extents, hits, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Box => Physics2D.OverlapBoxNonAlloc(position, Shape.Dimension.Size, offset.Rotation + Shape.Dimension.Rotation, hits, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    _ => throw Shape.ShapeType.BadValue(),
                };
            }

            public int GetCastAll(Dimension offset, RaycastHit2D[] hits)
            {
                Vector2 position = offset.Size;
                Direction cast = Cast;
                cast.Angle += offset.Rotation;
                Vector2 castDirection = cast.Vector2;
                return Shape.ShapeType switch
                {
                    ShapeTypes.Point => Physics2D.RaycastNonAlloc(position, castDirection, hits, Cast.Distance, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Circle => Physics2D.CircleCastNonAlloc(position, Shape.Dimension.Width, castDirection, hits, Cast.Distance, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Area => Physics2D.BoxCastNonAlloc(position - Shape.Dimension.Extents, position + Shape.Dimension.Extents, 0f, castDirection, hits, Cast.Distance, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    ShapeTypes.Box => Physics2D.BoxCastNonAlloc(position, Shape.Dimension.Size, offset.Rotation + Shape.Dimension.Rotation, castDirection, hits, Cast.Distance, QueryParams.LayerMask, QueryParams.MinDepth, QueryParams.MaxDepth),
                    _ => throw Shape.ShapeType.BadValue(),
                };
            }
        }
    }
}