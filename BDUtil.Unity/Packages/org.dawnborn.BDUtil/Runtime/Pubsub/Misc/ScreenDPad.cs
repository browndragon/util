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
        public Vector2 ScreenFactor => new(2f / UnityEngine.Screen.width, 2f / UnityEngine.Screen.height);
        public Rect ScreenZone => new(-Vector2.one, 2 * Vector2.one);
        protected void Update()
        {
            // From -1->+1.
            Vector2 mousePos = Vector2.Scale(ScreenFactor, UnityEngine.Input.mousePosition) - Vector2.one;
            Vector2 keyboardInput = new(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
            if (
                keyboardInput != default
                || !ScreenZone.Contains(mousePos)
                // It's not in a normalized circle centered on the screen...
                || mousePos.sqrMagnitude <= DeadZoneRadius * DeadZoneRadius
            )
            {
                // Mouse not on the active part of the screen, so go with the keyboard.
                Dir.Value = keyboardInput;
                return;
            }
            // This mightn't be right; we might want to take angle based on character position.
            float rawAngle = Vector2.SignedAngle(Vector2.right, mousePos);
            float angle = Mathf.Round(rawAngle / DeadAngleSize - .5f) * DeadAngleSize;
            Dir.Value = Vectors.OfAngle(angle);
        }
    }
}
