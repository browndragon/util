using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
{
    [AddComponentMenu("BDUtil/WorldMouse")]
    public class WorldMouse : MonoBehaviour
    {
        public Plane plane;
        new Camera camera;
        protected void Awake() => camera = Camera.main;
        protected void Update() => transform.position = camera.ScreenPointToRay(Input.mousePosition).Linecast(plane);
    }
}