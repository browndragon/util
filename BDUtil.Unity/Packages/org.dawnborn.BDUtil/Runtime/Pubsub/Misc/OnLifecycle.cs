using BDUtil.Math;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/OnLifecycle")]
    [Tooltip("Support publishing UnityEvents on various object lifecycle events. Particularly useful for overriding animation default state etc.")]
    public class OnLifecycle : MonoBehaviour
    {
        public UnityEvent OnStart_;
        public UnityEvent OnEnable_;
        public UnityEvent OnDisable_;
        public UnityEvent OnDestroy_;
        public Delay Delay;
        public UnityEvent After_;

        protected void Start()
        {
            OnStart_?.Invoke();
        }
        protected void OnEnable()
        {
            Delay.Reset();
            OnEnable_?.Invoke();
        }
        protected void OnDisable()
        {
            Delay.Stop();
            OnDisable_?.Invoke();
        }
        protected void OnDestroy()
        {
            OnDestroy_?.Invoke();
        }
        protected void Update()
        {
            if (Delay.IsEnded)
            {
                Delay.Stop();
                After_?.Invoke();
            }
        }

        // For objects which fade some time after creation
        internal void SelfDestruct() => Destroy(this);

        Animator animator;
        Animator Animator
        {
            get
            {
                if (animator == null) animator = GetComponent<Animator>();
                return animator;
            }
        }
        /// More generic/reflective support for animator bools. Could use triggers, but this is fine.
        public void SetAnimatorParamTrue(string name) => Animator.SetBool(name, true);
        public void SetAnimatorParamFalse(string name) => Animator.SetBool(name, false);
        public void SetAnimatorParamTwiddle(string name) => Animator.SetBool(name, Animator.GetBool(name));
    }
}