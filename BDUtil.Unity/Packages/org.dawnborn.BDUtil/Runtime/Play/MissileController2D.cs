using BDUtil.Clone;
using BDUtil.Math;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Play
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class MissileController2D : MonoBehaviour
    {
        public Delay Lifetimer = 1f;
        public bool IsSeeking;  // If is Seeking, calculate Offset from Target; otherwise,
        public Transform Target;
        public Vector3 Offset;
        Vector3 Acceleration;
        public float SmoothTime = .125f;
        public float OffsetZRot = 0f;

        public float Speed = 12f;
        public Delay Startup = Clock.FixedNow.StoppedDelayOf(.25f);
        public float Closing = .25f;
        public Easings.Enum StartEase = default;
        public Easings.Enum EndEase = default;

        public UnityEvent OnTarget = new();
        public bool ReachedTarget = false;

        new Rigidbody2D rigidbody;
        protected void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
        }
        protected void OnEnable()
        {
            Lifetimer.Reset();
            Startup.Reset();
            ReachedTarget = false;
        }
        protected void Update()
        {
            if (!Lifetimer)
            {
                Pool.main.Release(gameObject);
                return;
            }
            if (ReachedTarget) return;  // continue with same velocity, self destruct, who knows?
            if (!IsSeeking)
            {
                rigidbody.velocity = Vector3.SmoothDamp(rigidbody.velocity, Speed * Offset, ref Acceleration, SmoothTime, Speed / SmoothTime);
                return;
            }
            Vector3 targetVelocity = Offset + (Target == null ? Vector3.zero : Target.position) - transform.position;
            float length = targetVelocity.sqrMagnitude;
            if (length < .0001f)
            {
                ReachedTarget = true;
                OnTarget.Invoke();
                return;
            }
            length = Mathf.Sqrt(length);
            targetVelocity /= length;
            if (float.IsFinite(OffsetZRot))
            {
                float angle = Vector2.SignedAngle(Vector2.right, targetVelocity);
                transform.eulerAngles = new(0f, 0f, angle + OffsetZRot);
            }
            if (length <= Closing) length = EndEase.Invoke(length / Closing);
            else length = StartEase.Invoke(Startup.Ratio);
            rigidbody.velocity = length * targetVelocity;
        }
    }
}