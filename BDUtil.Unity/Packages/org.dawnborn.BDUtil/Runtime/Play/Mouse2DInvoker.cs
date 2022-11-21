using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Play
{
    [AddComponentMenu("BDUtil/Mouse2DInvoker")]
    [Tooltip("Support 2d mouse event->publish to topic or unity event")]
    [RequireComponent(typeof(Collider2D))]
    public class Mouse2DInvoker : MonoBehaviour
    {
        public UnityEvent OnEnter;
        public UnityEvent OnOver;
        public UnityEvent OnExit;
        public UnityEvent OnDown;
        public UnityEvent OnDrag;
        public UnityEvent OnUp;

        protected void OnMouseEnter() => OnEnter?.Invoke();
        protected void OnMouseOver() => OnOver?.Invoke();
        protected void OnMouseExit() => OnExit?.Invoke();
        protected void OnMouseDown() => OnDown?.Invoke();
        protected void OnMouseDrag() => OnDrag?.Invoke();
        protected void OnMouseUp() => OnUp?.Invoke();

        public void Log(string ident) => Debug.Log(ident);
        public void LogWithPos(string ident) => Debug.Log($"{ident}: {Input.mousePosition}");
    }
}