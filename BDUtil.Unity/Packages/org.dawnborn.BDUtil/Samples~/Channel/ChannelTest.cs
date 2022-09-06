using System.Collections;
using System.Collections.Generic;
using BDUtil.Pooling;
using BDUtil.Pubsub;
using UnityEngine;

namespace BDUtil
{
    public class ChannelTest : MonoBehaviour
    {
        public Topic OnCreate;
        public Topic OnDestroy;
        new Camera camera;
        public Clone Proto;
        void Awake() => camera = Camera.main;
        void Update()
        {
            if (!Input.GetMouseButtonUp(0)) return;
            Vector2 point = camera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D atPoint = Physics2D.OverlapPoint(point);
            if (atPoint != null)
            {
                Pools.main.Release(Proto, atPoint.GetComponent<Clone>());
                OnDestroy.Notify();
                return;
            }
            OnCreate.Notify();
            Clone cloned = Pools.main.Acquire(Proto);
            cloned.transform.SetPositionAndRotation(camera.ScreenToWorldPoint(Input.mousePosition).WithZ(0f), Quaternion.identity);
        }
    }
}