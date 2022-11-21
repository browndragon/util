using System.Collections;
using BDUtil.Math;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Play
{
    [AddComponentMenu("BDUtil/OnTimer")]
    [Tooltip("Support publishing UnityEvents based on a timer.")]
    public class OnTimer : MonoBehaviour
    {
        public bool Loop = true;
        public Delay Delay = 0f;
        public UnityEvent Event = new();
        int Counter = 0;
        protected void OnEnable() => StartCoroutine(Coroutine(Counter));
        protected void OnDisable() => Counter++;
        IEnumerator Coroutine(int counter)
        {
            do
            {
                Delay.Reset();
                do { yield return Delay.Now.GetYield(); } while (counter == Counter && Delay.Current);
                if (counter == Counter) Event?.Invoke();
            } while (counter == Counter && Loop);
        }
    }
}