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

        protected void Start() => OnStart_?.Invoke();
        protected void OnEnable() => OnEnable_?.Invoke();
        protected void OnDisable() => OnDisable_?.Invoke();
        protected void OnDestroy() => OnDestroy_?.Invoke();

        /// Support for animator bools. Could use triggers, but this is fine.
        Animator animator;
        Animator Animator => animator ??= GetComponent<Animator>().OrThrow();
        public void SetAnimatorParamTrue(string name) => Animator.SetBool(name, true);
        public void SetAnimatorParamFalse(string name) => Animator.SetBool(name, false);
    }
}