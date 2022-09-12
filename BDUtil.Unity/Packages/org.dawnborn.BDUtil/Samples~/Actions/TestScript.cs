using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [RequireComponent(typeof(Camera))]
    public class TestScript : MonoBehaviour
    {
        public ActionsHead ActionQueue;
        new Camera camera;
        void OnEnable()
        {
            camera = GetComponent<Camera>();
        }
        float Delay = .2f;
        float Next = 0f;
        void Update()
        {
            float timeNow = Time.time;
            if (timeNow < Next) return;
            Next = timeNow + Delay;
            Vector2 mouseWas = Input.mousePosition;
            ActionQueue.Push(() => Debug.Log($"{Time.time}: {mouseWas}@{timeNow}"));
        }
    }

}