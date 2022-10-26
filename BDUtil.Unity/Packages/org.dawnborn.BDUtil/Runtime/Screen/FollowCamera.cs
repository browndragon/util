using System;
using BDUtil;
using BDUtil.Math;
using BDUtil.Pubsub;
using BDUtil.Screen;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BDRPG.Screen
{
    [AddComponentMenu("BDUtil/FollowCamera")]
    [RequireComponent(typeof(Camera))]
    [Tooltip("Follows something -- the mouse, a specific character, etc.")]
    public class FollowCamera : MonoBehaviour
    {
        static readonly Rect unitRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);

        public interface IPointSource
        {
            Vector2 GetViewportPoint(Camera camera);
        }
        [Serializable]
        public struct MousePointSource : IPointSource
        {
            public Vector2 GetViewportPoint(Camera camera)
            {
                // Mouse is over a button; don't move.
                if (EventSystem.current?.IsPointerOverGameObject() ?? false) return .5f * Vector2.one;
                Vector2 pointer = camera.ScreenToViewportPoint(Input.mousePosition);
                // Mouse is off of the screen; don't move.
                if (!unitRect.Contains(pointer)) return .5f * Vector2.one;
                return pointer;
            }
        }
        [Serializable]
        public struct TransformPointSource : IPointSource
        {
            public Transform Transform;
            public Vector2 GetViewportPoint(Camera camera)
            {
                if (Transform == null) return .5f * Vector2.one;
                return camera.WorldToViewportPoint(Transform.position);
            }

        }
        [Serializable]
        public struct WorldPointSource : IPointSource
        {
            public Topic<Vector3> PointSource;
            public Vector2 GetViewportPoint(Camera camera) => camera.WorldToViewportPoint(PointSource.Value);
        }
        [Serializable]
        public struct GameObjectSource : IPointSource
        {
            public Topic<GameObject> PointSource;
            public Vector2 GetViewportPoint(Camera camera) => camera.WorldToViewportPoint(PointSource.Value.transform.position);
        }

        [Tooltip("What are we following? It's legal to change this during play.")]
        [SerializeReference, Subtype] public IPointSource PointSource = new MousePointSource();

        [Tooltip("Screen-center ratio to ignore movement; (1,1) would disable all, (0,0) none.")]
        public Vector2 DeadZone = .5f * Vector2.one;
        [Tooltip("Screen-center ratio to extend the curve @ max motion; if dead+max > 1, dead wins.")]
        public Vector2 MaxZone = .875f * Vector2.one;
        [Tooltip("Mouse ratio between dead & max -> speed ratio between 0 & GroundSpeed")]
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("Speed to move along ground to bring mouse back into dead zone")]
        public float GroundSpeed = 25;

        new Camera camera;
        bool suppressed;

        protected void OnEnable() => camera = Camera.main;

        protected void Update()
        {
            // This COULD be done with raycaster, but IMO not worth it.
            // It's really for debugging anyway.
            if (Input.GetMouseButtonUp(2)) suppressed = !suppressed;
            if (Input.GetMouseButtonUp(0)) suppressed = false;
            if (Input.GetMouseButtonUp(1)) suppressed = false;
        }

        public Vector2 Velocity;
        public Vector2 Viewport0;
        public Vector2 SpeedRatio;
        public bool IsInDeadZone;
        protected void FixedUpdate()
        {
            if (PointSource == null) return;
            if (suppressed) return;
            Vector2 trackedViewport = PointSource.GetViewportPoint(camera);
            // Okay, so:
            Viewport0 = trackedViewport - .5f * Vector2.one;
            // We're now x&y in [-.5f,+.5f].
            Vector2 halfDead = DeadZone / 2;
            if (Viewport0.x.IsInRange(-halfDead.x, +halfDead.x)) Viewport0.x = 0f;
            if (Viewport0.y.IsInRange(-halfDead.y, +halfDead.y)) Viewport0.y = 0f;
            if (IsInDeadZone = Viewport0 == default) return;

            // Okay, we're NOT in the dead zone. Converge!
            SpeedRatio = Viewport0;
            Vector2 sign = new(Mathf.Sign(SpeedRatio.x), Mathf.Sign(SpeedRatio.y));
            Vector2 abs = new(SpeedRatio.x * sign.x, SpeedRatio.y * sign.y);
            Vector2 halfMax = MaxZone / 2;
            if (abs.x > halfMax.x) SpeedRatio.x = sign.x * 1f;
            else SpeedRatio.x = sign.x * Curve.Evaluate((abs.x - halfDead.x) / (halfMax.x - halfDead.x));
            if (abs.y > halfMax.y) SpeedRatio.y = sign.y * 1f;
            else SpeedRatio.y = sign.y * Curve.Evaluate((abs.y - halfDead.y) / (halfMax.y - halfDead.y));

            float speed = GroundSpeed * SpeedRatio.magnitude;
            camera.MoveAlongXY(
                Vector2.Scale(
                    new(UnityEngine.Screen.width, UnityEngine.Screen.height),
                    trackedViewport
                ),
                ref Velocity,
                maxSpeed: speed
            );
            if (!SceneBounds.Bounds.Contains(camera.transform.position))
            {
                Vector3 inBounds = SceneBounds.Bounds.ClosestPoint(camera.transform.position);
                camera.MoveAlongXYDelta(inBounds - camera.transform.position, ref Velocity);
            }
        }
    }
}