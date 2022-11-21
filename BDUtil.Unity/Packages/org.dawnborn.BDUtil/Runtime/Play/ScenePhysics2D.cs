using BDUtil.Clone;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Play
{
    public class ScenePhysics2D : MonoBehaviour
    {
        public Bounds SceneBounds = new(default, new(10, 10, 10));
        Bounds hadBounds;
        public enum OutOfBounds
        {
            Ignore = default,
            Bounce,
            Pool,
            Destroy,
        }
        public OutOfBounds OutOfBound;
        public Vector2 Gravity;
        Vector2 hadGravity;

        public CollisionMatrix CollisionMatrix;
        protected CollisionMatrix hadCollisionMatrix;
        protected void Reset()
        {
            Gravity = Physics2D.gravity;
            CollisionMatrix.SetFromPhysics2D();
        }
        protected void OnEnable()
        {
            hadGravity = Physics2D.gravity;
            Physics2D.gravity = Gravity;
            hadBounds = Screen.SceneBounds.Bounds;
            if (!SceneBounds.center.HasNaN()) Screen.SceneBounds.Bounds = SceneBounds;
            hadCollisionMatrix.SetFromPhysics2D();
            CollisionMatrix.SetToPhysics2D();
        }
        protected void OnDisable()
        {
            Physics2D.gravity = hadGravity;
            if (!SceneBounds.center.HasNaN()) Screen.SceneBounds.Bounds = hadBounds;
            hadCollisionMatrix.SetToPhysics2D();
        }
        protected void OnDrawGizmosSelected()
        {
            Bounds draw = SceneBounds;
            if (Application.isPlaying)
            {
                Gizmos.color = new(0f, 1f, 0f, .7f);
                draw = Screen.SceneBounds.Bounds;
            }
            else if (!draw.center.HasNaN()) Gizmos.color = new(0f, .8f, 0f, .7f);
            else
            {
                Gizmos.color = new(0f, .7f, .2f, .7f);
                draw = Screen.SceneBounds.Bounds;
            }
            Gizmos.DrawWireCube(draw.center, draw.size);
        }
        protected void Update()
        {
            switch (OutOfBound)
            {
                case OutOfBounds.Ignore: return;
                case OutOfBounds.Bounce:
                    foreach (Rigidbody2D rigidbody in FindObjectsOfType<Rigidbody2D>())
                    {
                        if (Screen.SceneBounds.Bounds.Contains(rigidbody.transform.position)) continue;
                        Screen.SceneBounds.BounceIn(rigidbody.gameObject);
                    }
                    break;
                case OutOfBounds.Destroy:
                    foreach (Rigidbody2D rigidbody in FindObjectsOfType<Rigidbody2D>())
                    {
                        if (Screen.SceneBounds.Bounds.Contains(rigidbody.transform.position)) continue;
                        Destroy(rigidbody.gameObject);
                    }
                    break;
                case OutOfBounds.Pool:
                    foreach (Rigidbody2D rigidbody in FindObjectsOfType<Rigidbody2D>())
                    {
                        Vector3 rtp = rigidbody.transform.position;
                        if (Screen.SceneBounds.Bounds.Contains(rtp)) continue;
                        Debug.Log($"{rigidbody}@{rtp} outside {Screen.SceneBounds.Bounds}, killing", rigidbody);
                        Pool.main.Release(rigidbody.gameObject);
                    }
                    break;
            }

        }
    }
}