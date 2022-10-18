using BDUtil.Math;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Pubsub
{
    public class DirTopic : SingletonMB<DirTopic>
    {
        public Vector2 DeadZoneSize = new(1f, 1f);
        public Val<Vector2> Dir;
        public Rect ScreenZone => new(Vector2.zero, 2 * Vector2.one);
        public Rect DeadZone => new(Vector2.zero, DeadZoneSize);
        protected void Update()
        {
            Vector2 screenFactor = new(2f / UnityEngine.Screen.width, 2f / UnityEngine.Screen.height);
            // From -1->+1.
            Vector2 mousePos = Vector2.Scale(screenFactor, UnityEngine.Input.mousePosition) - Vector2.one;
            Vector2 keyboardInput = new(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
            if (
                keyboardInput != default
                || !ScreenZone.Contains(mousePos)
                || DeadZone.Contains(mousePos)
            )
            {
                // Mouse not on the active part of the screen, so go with the keyboard.
                Dir.Value = keyboardInput;
                return;
            }
            Vector2 sign = new(Mathf.Sign(mousePos.x), Mathf.Sign(mousePos.y));
            Dir.Value = sign.normalized;
        }
    }
}
