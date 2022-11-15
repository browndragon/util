using BDUtil.Math;
using BDUtil.Pubsub;
using UnityEngine;
using UnityEngine.Events;

namespace BDRPG.Screen
{
    [AddComponentMenu("BDUtil/Spotlight")]
    [Tooltip("Follows a specific game object and externally modifies things wrt its state.")]
    public class Spotlight : MonoBehaviour
    {
        [SerializeField] public Val<GameObject> target;
        public GameObject Target
        {
            get => target.Value; set
            {
                target.Value = value;
                CalledInactiveOnce = false;
            }
        }
        public UnityEvent<GameObject> OnUpdate;
        public Delay Delay = .5f;
        public UnityEvent OnInactive;
        public bool CallInactiveEachFrame = false;
        public bool CalledInactiveOnce = false;

        protected void Update()
        {
            if (Target && Target.activeInHierarchy)
            {
                CalledInactiveOnce = false;
                Delay.Reset();
                OnUpdate?.Invoke(Target);
            }
            if (Delay) return;
            if (CalledInactiveOnce && !CallInactiveEachFrame) return;
            CalledInactiveOnce = true;
            OnInactive?.Invoke();
        }
    }
}