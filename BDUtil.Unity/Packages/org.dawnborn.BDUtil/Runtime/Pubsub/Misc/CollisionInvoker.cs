using System;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    public abstract class CollisionInvoker : MonoBehaviour
    {
        [Serializable]
        public struct EnterStayExit<T>
        {
            public UnityEvent<T> OnEnter;
            public UnityEvent<T> OnStay;
            public UnityEvent<T> OnExit;
            public Action Subscribe<V>(EnterStayExit<V> other, Func<T, V> func)
            {
                Action unsubEnter = OnEnter.Subscribe(t => other.OnEnter.Invoke(func(t)));
                Action unsubStay = OnStay.Subscribe(t => other.OnStay.Invoke(func(t)));
                Action unsubExit = OnExit.Subscribe(t => other.OnExit.Invoke(func(t)));
                return () =>
                {
                    unsubEnter();
                    unsubStay();
                    unsubExit();
                };
            }
        }
        protected static EnterStayExit<T> New<T>() => new()
        {
            OnEnter = new(),
            OnStay = new(),
            OnExit = new(),
        };
    }
    public abstract class CollisionInvoker<T> : CollisionInvoker
    where T : Component
    {
        public EnterStayExit<T> OnCollider = New<T>();
        public EnterStayExit<GameObject> OnGameObject = New<GameObject>();
        readonly Disposes.All unsubscribe = new();

        protected void OnEnable() => unsubscribe.Add(OnCollider.Subscribe(OnGameObject, collider => collider.gameObject));
        protected void OnDisable() => unsubscribe.Dispose();
    }
}