using System.Collections;
using System.Collections.Generic;
using BDUtil.Math;
using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Play
{
    [Tooltip("Manages a specialized unidirectional 'collider'. ")]
    [AddComponentMenu("BDUtil/Hitbox")]
    public class Hitbox : MonoBehaviour
    {
        public Vector2 Offset;
        public Metaphysics.Query Query = Metaphysics.Query.Default;
        public bool IgnoreSameTag = true;
        public Store<HashSet<string>, string> IgnoreTags = new();
        public UnityEvent<Collider2D> OnTrigger = new();
        public Clock Clock;
        protected void OnEnable() => StartCoroutine(DoUpdate());
        IEnumerator DoUpdate()
        {
            while (true)
            {
                yield return Clock.GetYield();
                Vector2 position = (Vector2)transform.position + Offset;
                for (int i = 0, casted = Query.GetOverlapAll(position, Metaphysics.ScratchColliders); i < casted; ++i)
                {
                    Collider2D collider = Metaphysics.ScratchColliders[i];
                    Metaphysics.ScratchColliders[i] = default;
                    if (gameObject == collider.gameObject) continue;
                    if (IgnoreSameTag && collider.CompareTag(tag)) continue;
                    if (IgnoreTags.Collection.Contains(collider.tag)) continue;
                    OnTrigger?.Invoke(collider);
                }
            }
        }
        protected virtual void OnDrawGizmosSelected()
        {
            void Draw(Vector3 point)
            {
                switch (Query.Shape.ShapeType)
                {
                    case Metaphysics.ShapeTypes.Point:
                        Gizmos.DrawIcon(point, "icon dropdown@2x");
                        break;
                    case Metaphysics.ShapeTypes.Circle:
                        Gizmos.DrawWireSphere(point, Query.Shape.Dimension.Width);
                        break;
                    case Metaphysics.ShapeTypes.Area:
                        // We should probably counter-rotate, since area operates in flat coords...?
                        Gizmos.DrawWireCube(point, Query.Shape.Dimension.Size);
                        break;
                    case Metaphysics.ShapeTypes.Box:
                        Gizmos.matrix *= Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, Query.Shape.Dimension.Rotation));
                        Gizmos.DrawWireCube(point, Query.Shape.Dimension.Size);
                        break;
                    default: throw Query.Shape.ShapeType.BadValue();
                }
            }
            Matrix4x4 wasMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.Translate(Offset) * transform.localToWorldMatrix;
            if (Query.Cast.Distance > 0f)
            {
                Vector3 end = Mathf.Clamp(Query.Cast.Distance, .01f, 1000f) * Query.Cast.Vector2;
                Gizmos.DrawLine(Vector3.zero, end);
                Draw(Vector3.zero);
                Draw(end);
            }
            else Draw(Vector3.zero);
            Gizmos.matrix = wasMatrix;
        }
    }
}