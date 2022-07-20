using System;
using UnityEngine;

namespace BDUtil
{
    /// Syntax sugar for getting input positions in the world space.
    /// Assumes you want it at z=0, or else you would do the math yourself :-D
    public static class CameraExt
    {
        /// Gets the `Input.mousePosition` in world space along z=0.
        public static Vector3 GetWorldMousePosition(this Camera main, float z = 0)
        => main.ScreenPointToRay(Input.mousePosition).AtZ(z);

        /// Gets the `Input.touch` `rawPosition` in world space along z=0.
        public static Vector3 GetWorldTouchPosition(this Camera main, int touch, float z = 0)
        => main.ScreenPointToRay(Input.touches[touch].rawPosition).AtZ(z);
    }
}