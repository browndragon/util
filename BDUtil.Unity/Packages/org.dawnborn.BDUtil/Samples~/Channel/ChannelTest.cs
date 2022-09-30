using System.Diagnostics.CodeAnalysis;
using BDUtil.Math;
using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil.Serialization
{
    public class ChannelTest : MonoBehaviour
    {
        public IntTopic NumCreated;
        public IntTopic NumDestroyed;
        public GameObject Proto;
        new Camera camera;

        [SuppressMessage("IDE", "IDE0051")]
        void Awake() => camera = Camera.main;
        [SuppressMessage("IDE", "IDE0051")]
        void Update()
        {
            if (!Input.GetMouseButtonUp(0)) return;
            Vector2 point = camera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D atPoint = Physics2D.OverlapPoint(point);
            if (atPoint != null)
            {
                NumDestroyed.Value++;
                Clone.Pool.main.Release(atPoint.gameObject);
                return;
            }
            NumCreated.Value++;
            GameObject cloned = Clone.Pool.main.Acquire(Proto);
            cloned.transform.SetPositionAndRotation(camera.ScreenToWorldPoint(Input.mousePosition).WithZ(0f), Quaternion.identity);
        }
    }
}