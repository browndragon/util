using BDUtil.Math;
using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil.Play
{
    // Nothing fancy, just drives mousepos|keyboard->topic.
    [AddComponentMenu("BDUtil/ScreenDPad")]
    public class ScreenDPad : MonoBehaviour
    {
        public float DeadZoneRadius = .5f;
        public float DeadAngleSize = 15f;

        public Val<Vector2> Dir;
        public Val<bool> Fire1;
        public Val<bool> Fire2;
        public Val<bool> Fire3;

        public Vector2 ScreenFactor => new(2f / UnityEngine.Screen.width, 2f / UnityEngine.Screen.height);
        public Rect ScreenZone => new(-Vector2.one, 2 * Vector2.one);
        protected void Update()
        {
            UpdateDirection();
            UpdateFire();
        }
        void UpdateFire()
        {
            bool fire1 = false, fire2 = false, fire3 = false;

            if (Input.GetButton("Fire1")) fire1 = true;
            if (Input.GetButton("Fire2")) fire2 = true;
            if (Input.GetButton("Fire3")) fire3 = true;
            if (Input.GetMouseButtonDown(2)) fire3 = true;
            if (Input.GetMouseButtonDown(1)) fire2 = true;
            if (Input.GetMouseButtonDown(0)) fire1 = true;
            switch (Input.touchCount)
            {
                case 0: break;
                case 1: fire1 = true; break;
                case 2: fire2 = true; break;
                case 3: fire3 = true; break;
                default: break;
            }
            Fire1.Value = fire1;
            Fire2.Value = fire2;
            Fire3.Value = fire3;
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
