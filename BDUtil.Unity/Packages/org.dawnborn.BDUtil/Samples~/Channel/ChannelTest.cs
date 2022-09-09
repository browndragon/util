using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil.Serialization
{
    public class ChannelTest : MonoBehaviour
    {
        public Topic OnCreate;
        public Topic OnDestroy;
        new Camera camera;
        public Ref<Clone> Proto;
        void Awake() => camera = Camera.main;
        void Update()
        {
            if (!Input.GetMouseButtonUp(0)) return;
            Vector2 point = camera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D atPoint = Physics2D.OverlapPoint(point);
            if (atPoint != null)
            {
                Clone.Release(atPoint);
                OnDestroy.Publish();
                return;
            }
            OnCreate.Publish();
            Clone cloned = Clone.Acquire(Proto);
            cloned.transform.SetPositionAndRotation(camera.ScreenToWorldPoint(Input.mousePosition).WithZ(0f), Quaternion.identity);
        }
    }
}