using BDUtil;
using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDRPG.Screen
{
    [AddComponentMenu("BDUtil/FollowCamera")]
    [RequireComponent(typeof(Camera))]
    [Tooltip("Follows something -- the mouse, a specific character, etc.")]
    public class FollowCamera : MonoBehaviour
    {
        static readonly Rect unitRect = Rect.MinMaxRect(0f, 0f, 1f, 1f);
        public float CamSmooth = .125f;
        public bool FollowMouse = true;
        [Tooltip("A point in the viewport space where -1 is left/bottom, +1 is right/top")]
        [field: SerializeField] public Vector2 ViewportPoint { get; private set; }

        public void SetViewportPointFromWorldPoint(Vector3 worldPoint)
        => ViewportPoint = 2 * camera.WorldToViewportPoint(worldPoint) - Vector3.one;
        public void SetViewportPointFromObject(GameObject gameObject)
        => SetViewportPointFromWorldPoint(gameObject.transform.position);
        public void SetViewportPointFromComponent(Component component)
        => SetViewportPointFromWorldPoint(component.transform.position);

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

        public Vector3 Velocity;
        public Vector2 SpeedRatio;
        public bool IsInDeadZone;
        protected void FixedUpdate()
        {
            if (suppressed) return;
            if (FollowMouse)
            {
                ViewportPoint = 2 * (Vector2)camera.ScreenToViewportPoint(Input.mousePosition) - Vector2.one;
            }
            // We're now x&y in [-.5f,+.5f].
            Vector2 halfDead = DeadZone / 2;
            SpeedRatio = ViewportPoint;
            if (SpeedRatio.x.IsInRange(-halfDead.x, +halfDead.x)) SpeedRatio.x = 0f;
            if (SpeedRatio.y.IsInRange(-halfDead.y, +halfDead.y)) SpeedRatio.y = 0f;
            if (IsInDeadZone = SpeedRatio == default)
            {
                camera.MoveAlongXYDelta(
                    default,
                    ref Velocity,
                    CamSmooth  //,
                               // maxSpeed: GroundSpeed
                );
                return;
            }

            // Okay, we're NOT in the dead zone. Converge!
            // Vector2 sign = new(Mathf.Sign(SpeedRatio.x), Mathf.Sign(SpeedRatio.y));
            // Vector2 abs = new(SpeedRatio.x * sign.x, SpeedRatio.y * sign.y);
            // Vector2 halfMax = MaxZone / 2;
            // if (abs.x > halfMax.x) SpeedRatio.x = sign.x * 1f;
            // else SpeedRatio.x = (abs.x - halfDead.x) / (halfMax.x - halfDead.x);
            // if (abs.y > halfMax.y) SpeedRatio.y = sign.y * 1f;
            // else SpeedRatio.y = (abs.y - halfDead.y) / (halfMax.y - halfDead.y);

            // float speed = GroundSpeed * Curve.Evaluate(SpeedRatio.magnitude);
            Vector3 worldpoint = camera.LookingAt((ViewportPoint + Vector2.one) / 2);
            if (!SceneBounds.Bounds.Contains(worldpoint))
            {
                worldpoint = SceneBounds.Bounds.ClosestPoint(worldpoint);
            }
            camera.MoveAlongXYDelta(
                worldpoint - camera.LookingAt(),
                ref Velocity,
                CamSmooth  // ,
                           // speed
            );
        }
    }
}