using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    // Nothing fancy, just drives mousepos|keyboard->topic.
    [AddComponentMenu("BDUtil/ScreenDPad")]
    public class ScreenDPad : MonoBehaviour
    {
        public float DeadZoneRadius = .5f;
        public float DeadAngleSize = 15f;
        public Val<Vector2> Dir;
        public Val Fire1;
        public Val Fire2;
        public Val Fire3;
        public Vector2 ScreenFactor => new(2f / UnityEngine.Screen.width, 2f / UnityEngine.Screen.height);
        public Rect ScreenZone => new(-Vector2.one, 2 * Vector2.one);
        protected void Update()
        {
            UpdateDirection();
            UpdateFire();
        }
        void UpdateFire()
        {
            if (Input.GetButton("Fire3")) Fire3.Topic.Publish();
            else if (Input.GetButton("Fire2")) Fire2.Topic.Publish();
            else if (Input.GetButton("Fire1")) Fire1.Topic.Publish();
            else if (Input.touchCount >= 4) Fire3.Topic.Publish();
            else if (Input.touchCount >= 3) Fire2.Topic.Publish();
            else if (Input.touchCount >= 2) Fire1.Topic.Publish();
            else if (Input.touchCount <= 0)
            {
                if (Input.GetMouseButtonDown(2)) Fire3.Topic.Publish();
                else if (Input.GetMouseButtonDown(1)) Fire2.Topic.Publish();
                else if (Input.GetMouseButtonDown(0)) Fire1.Topic.Publish();
            }
        }
        void UpdateDirection()
        {
            Vector2 keyboardInput = new(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
            if (keyboardInput != default) { Dir.Value = keyboardInput.normalized; return; }
            // From -1->+1.
            Vector2 mousePos = default;
            if (Input.touchCount >= 2)
            {
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    Touch touch = Input.GetTouch(i);
                    Vector2 touchPos = Vector2.Scale(ScreenFactor, touch.position) - Vector2.one;
                    if (touchPos.x * touchPos.x > mousePos.x * mousePos.x) mousePos.x = touchPos.x;
                    if (touchPos.y * touchPos.y > mousePos.y * mousePos.y) mousePos.y = touchPos.y;
                }
            }
            else
            {
                // since mousePos *is* touchpos[0]...
                mousePos = Vector2.Scale(ScreenFactor, Input.mousePosition) - Vector2.one;
            }
            if (!ScreenZone.Contains(mousePos)) { Dir.Value = default; return; }
            if (mousePos.sqrMagnitude <= DeadZoneRadius * DeadZoneRadius) { Dir.Value = default; return; }
            // This mightn't be right; we might want to take angle based on character position.
            float rawAngle = Vector2.SignedAngle(Vector2.right, mousePos);
            float angle = Mathf.Round(rawAngle / DeadAngleSize - .5f) * DeadAngleSize;
            Dir.Value = Vectors.OfAngle(angle);
        }
    }
}
