using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil
{
    [RequireComponent(typeof(Collider2D))]
    public class SubclassExample : MonoBehaviour, SubclassExample.ILog
    {
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

        public float Duration;
        [SerializeReference, Subclass]
        public Easings.IEase Easer;
        public interface ITarget
        {
            Vector3 Get(Vector3 value);
        }
        [SerializeReference, Subclass(Default = typeof(MouseX))]
        public ITarget Target;

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

        [SerializeReference, Subclass]
        public List<ILog> Loggers;

        public void LogNow()
        {
            foreach (ILog logger in Loggers) logger.LogNow();
        }

        public void Update()
        {
            if (!Input.GetMouseButtonUp(0)) return;
            LogNow();
            if (Target != null) StartCoroutine(Tween().GetEnumerator());
        }
        IEnumerable Tween()
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = Target.Get(transform.position);
            IArith<Vector3> arith = Arith<Vector3>.Default;

            for (float elapsed = 0f;
                elapsed < Duration;
                elapsed += Time.deltaTime)
            {
                transform.position = arith.Lerp(startPos, endPos, Easer.ClampInvoke(elapsed / Duration));
                yield return null;
            }
            transform.position = endPos;
        }
    }
}