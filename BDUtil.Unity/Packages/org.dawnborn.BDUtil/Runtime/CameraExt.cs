using System;
using UnityEngine;

namespace BDUtil
{
    /// Syntax sugar for getting input positions in the world space.
    public static class CameraExt
    {
        /// Gets the `Input.mousePosition` in world space.
        public static Vector3 GetWorldMousePosition(this Camera main)
        => main.ScreenToWorldPoint(Input.mousePosition);

        /// Gets the `Input.touch` `rawPosition` in world space.
        public static Vector3 GetWorldTouchPosition(this Camera main, int touch)
        => main.ScreenToWorldPoint(Input.touches[touch].rawPosition);
    }
}