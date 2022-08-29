using UnityEngine;

namespace BDUtil
{
    public class ChannelTest : MonoBehaviour
    {
        public Holder<Pubsub.Ping> OnCreate;
        public Holder<Pubsub.Ping> OnDestroy;
        new Camera camera;
        public Transform Proto;
        void Awake() => camera = Camera.main;
        void Update()
        {
            if (!Input.GetMouseButtonUp(0)) return;
            Vector2 point = camera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D atPoint = Physics2D.OverlapPoint(point);
            if (atPoint != null)
            {
                Destroy(atPoint.gameObject);
                OnDestroy.Value.Invoke();
                return;
            }
            OnCreate.Value.Invoke();
            Instantiate(Proto, camera.ScreenToWorldPoint(Input.mousePosition).WithZ(0f), Quaternion.identity);
        }
    }
}