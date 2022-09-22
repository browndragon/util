using System.Diagnostics.CodeAnalysis;
using BDUtil.Math;
using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil.Serialization
{
    public class ChannelTest : MonoBehaviour
    {
        public Val<int> NumCreated = new();
        public Val<int> NumDestroyed = new();

        new Camera camera;
        public Ref<Clone> Proto;

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
                Clone.Release(atPoint);
                return;
            }
            NumCreated.Value++;
            Clone cloned = Clone.Acquire(Proto);
            cloned.transform.SetPositionAndRotation(camera.ScreenToWorldPoint(Input.mousePosition).WithZ(0f), Quaternion.identity);
        }
    }
}