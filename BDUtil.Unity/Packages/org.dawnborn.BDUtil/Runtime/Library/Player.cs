using System;
using BDUtil.Math;
using BDUtil.Pubsub;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Library
{
    public class Player : MonoBehaviour, OnState.IEnter, OnState.IExit
    {
        [SerializeField] protected Invokable.Layout buttons;

        public interface IPlayable
        {
            float PlayOn(Player player);
        }
        public enum Strategies
        {
            Random = default,
            RoundRobin
        }
        public enum Automation
        {
            None = default,
            Continue,
            Startup,
        }
        public Strategies Strategy;
        public bool CanInterrupt = true;
        /// Secretly: Library<out IPlayable>, but we can't say that.
        public Library Library;
        // Initial/ongoing?
        public string Category = "";
        public Automation Automation_;
        // For round robin, lets you specify a start offset.
        public int index = -1;
        Timer Delay = 0f;
        public void PlayByCategoryForce(string tag, bool forceInterrupt)
        {
            if (tag == null) return;
            if (Delay.IsRunning && !CanInterrupt && !forceInterrupt) return;
            Library.ICategory category = Library.GetICategory(tag);
            if (category == null) return;
            index = Strategy switch
            {
                Strategies.Random => category.GetRandom(),
                Strategies.RoundRobin => (index + 1).PosMod(category.Count),
                _ => throw new NotImplementedException($"Unrecognized {Strategy}"),
            };
            object entry = category[index.CheckRange(0, category.Count)];
            Delay = new Timer(((IPlayable)entry).PlayOn(this));
            if (Automation_ == Automation.Startup) Automation_ = Automation.Continue;
        }
        public void PlayByCategory(string tag) => PlayByCategoryForce(tag, false);
        [Invokable]
        public void PlayCurrentCategory() => PlayByCategory(Category);
        protected void Update()
        {
            if (Delay.IsRunning) return;
            switch (Automation_)
            {
                case Automation.Continue: break;
                default: return;
            }
            PlayCurrentCategory();
        }

        public void SetAnimatorParamTrue(string param)
        => GetComponent<Animator>().SetBool(param, true);
        public void SetAnimatorParamFalse(string param)
        => GetComponent<Animator>().SetBool(param, false);
        public void SetAnimatorParamToggle(string param)
        {
            Animator animator = GetComponent<Animator>();
            animator.SetBool(param, !animator.GetBool(param));
        }
        public void SetAnimatorParamIntZero(string param)
        {
            Animator animator = GetComponent<Animator>();
            animator.SetInteger(param, 0);
        }
        public void SetAnimatorParamIntIncr(string param)
        {
            Animator animator = GetComponent<Animator>();
            animator.SetInteger(param, animator.GetInteger(param) + 1);
        }
        public void SetAnimatorParamIntDecr(string param)
        {
            Animator animator = GetComponent<Animator>();
            animator.SetInteger(param, animator.GetInteger(param) - 1);
        }
        public void SetAnimatorParamFloatZero(string param)
        {
            Animator animator = GetComponent<Animator>();
            animator.SetFloat(param, 0f);
        }

        void OnState.IEnter.OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => PlayByCategory(Library.GetTagStringFromHash(stateInfo, false));

        void OnState.IExit.OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => PlayByCategory(Library.GetTagStringFromHash(stateInfo, true));
    }
}
