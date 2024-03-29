using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Serialization
{
    [RequireComponent(typeof(Collider2D))]
    public class SubtypeExample : MonoBehaviour
    {
        public Delay Duration = .5f;
        public Easings.Enum Easer;
        public interface ITarget
        {
            Vector3 Get(Vector3 value);
        }
        public Subtype<ITarget> Target = typeof(MouseX);

        [Serializable]
        public struct MouseX : ITarget
        {
            public Vector3 Get(Vector3 pos)
            {
                Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.x = mPos.x;
                return pos;
            }
        }
        [Serializable]
        public struct MouseY : ITarget
        {
            public Vector3 Get(Vector3 pos)
            {
                Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.y = mPos.y;
                return pos;
            }
        }

        public interface ILog
        {
            void LogNow();
        }

        [Serializable]
        public struct LogConst : ILog
        {
            public string Const;
            public void LogNow() => Debug.Log(Const);
        }

        [Serializable]
        public struct LogAffix : ILog
        {
            public string Pre;
            public string Post;
            public void LogNow() => Debug.Log($"{Pre}{Post}");
        }

        [SerializeReference, Subtype]
        public List<ILog> Loggers;

        public void LogNow()
        {
            foreach (ILog logger in Loggers) logger.LogNow();
        }

        public void Update()
        {
            if (!Input.GetMouseButtonUp(0)) return;
            LogNow();
            ITarget instance = Target.CreateInstance();
            if (instance != null) StartCoroutine(Tween(instance));
        }
        IEnumerator Tween(ITarget instance)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = instance.Get(transform.position);
            IArith<Vector3> arith = Arith<Vector3>.Default;
            return Duration.Foreach(
                tick => transform.position = arith.Lerp(startPos, endPos, Easer.ClampInvoke(tick))
            );
        }
    }
}