using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
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

        void OnMouseEnter() => OnEnter?.Invoke();
        void OnMouseOver() => OnOver?.Invoke();
        void OnMouseExit() => OnExit?.Invoke();
        void OnMouseDown() => OnDown?.Invoke();
        void OnMouseDrag() => OnDrag?.Invoke();
        void OnMouseUp() => OnUp?.Invoke();

        public void Log(string ident) => Debug.Log(ident);
        public void LogWithPos(string ident) => Debug.Log($"{ident}: {Input.mousePosition}");
    }
}