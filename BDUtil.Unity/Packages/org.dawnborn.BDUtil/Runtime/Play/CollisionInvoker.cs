using System;
using System.Collections.Generic;
using BDUtil.Fluent;
using BDUtil.Pubsub;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Play
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
        /// Whatever my tag is, ignore it.
        public bool SkipSelfTag;
        /// And also ignore these tags (like: terrain -- though this could also use layers.)
        public Serialization.Store<HashSet<string>, string> SkipTags = new();
        public EnterStayExit<T> OnCollider = New<T>();
        public EnterStayExit<GameObject> OnGameObject = New<GameObject>();
        readonly Disposes.All unsubscribe = new();

        protected void OnEnable() => unsubscribe.Add(OnCollider.Subscribe(OnGameObject, collider => collider.gameObject));
        protected void OnDisable() => unsubscribe.Dispose();

        protected bool DoCollide(GameObject other)
        {
            string tag = other.tag;
            if (gameObject.CompareTag(tag) && SkipSelfTag)
            {
                Debug.Log($"Skipping collision {gameObject}<->{other}; skipselftag");
                return false;
            }
            if (SkipTags.Collection.Contains(other.tag))
            {
                Debug.Log($"Skipping collision {gameObject}<->{other}; {other.tag} in {SkipTags.Collection.Summarize()}");
                return false;
            }
            Debug.Log($"Enabling collision {gameObject}<->{other}");
            return true;
        }
    }
}