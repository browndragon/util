using System.Diagnostics.CodeAnalysis;
using BDUtil;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BDRPG.Screen
{
    /// Creates a camera with a central dead zone, and then outside of that an acceleration curve that attempts to slide towards the mouse direction.
    [AddComponentMenu("BDUtil/FollowCamera")]
    [RequireComponent(typeof(Camera))]
    public class FollowCamera : MonoBehaviour
    {
        static readonly Rect unitRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);

        [Tooltip("Screen-center ratio to ignore movement; (1,1) would disable all, (0,0) none.")]
        public Vector2 DeadZone = .5f * Vector2.one;
        [Tooltip("Screen-center ratio to extend the curve @ max motion; if dead+max > 1, dead wins.")]
        public Vector2 MaxZone = .875f * Vector2.one;
        [Tooltip("Mouse ratio between dead & max -> speed ratio between 0 & GroundSpeed")]
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [Tooltip("Speed to move along ground to bring mouse back into dead zone")]
        public float GroundSpeed = 25;

        new Camera camera;
        EventSystem eventSystem;
        bool suppressed;
        bool tempSuppressed;

        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable()
        {
            camera = Camera.main;
            eventSystem = EventSystem.current;
        }

        [SuppressMessage("IDE", "IDE0051")]
        void Update()
        {
            // This COULD be done with raycaster, but IMO not worth it.
            // It's really for debugging anyway.
            if (Input.GetMouseButtonUp(2)) suppressed = !suppressed;
            if (Input.GetMouseButtonUp(0)) suppressed = false;
            if (Input.GetMouseButtonUp(1)) suppressed = false;
            tempSuppressed = eventSystem?.IsPointerOverGameObject() ?? false;
        }

        [SuppressMessage("IDE", "IDE0051")]
        void LateUpdate()
        {
            if (suppressed) return;
            if (tempSuppressed) return;
            Vector2 mouseViewport = camera.ScreenToViewportPoint(Input.mousePosition);
            // Mouse is off of the screen; don't move.
            if (!unitRect.Contains(mouseViewport)) return;
            Vector2 ratio = mouseViewport - .5f * Vector2.one;
            // We're now x&y in [-.5f,+.5f].
            Vector2 halfDead = DeadZone / 2;
            if (ratio.x.IsInRange(-halfDead.x, +halfDead.x)) ratio.x = 0f;
            if (ratio.y.IsInRange(-halfDead.y, +halfDead.y)) ratio.y = 0f;
            if (ratio.x == 0f && ratio.y == 0f) return;

            Vector2 sign = new(Mathf.Sign(ratio.x), Mathf.Sign(ratio.y));
            Vector2 abs = new(ratio.x * sign.x, ratio.y * sign.y);
            Vector2 halfMax = MaxZone / 2;
            if (abs.x > halfMax.x) ratio.x = sign.x * 1f;
            else ratio.x = sign.x * Curve.Evaluate((abs.x - halfDead.x) / (halfMax.x - halfDead.x));
            if (abs.y > halfMax.y) ratio.y = sign.y * 1f;
            else ratio.y = sign.y * Curve.Evaluate((abs.y - halfDead.y) / (halfMax.y - halfDead.y));

            float speed = Time.deltaTime * GroundSpeed * ratio.magnitude;
            camera.MoveAlongXY(Input.mousePosition, speed, true);
        }
    }
}