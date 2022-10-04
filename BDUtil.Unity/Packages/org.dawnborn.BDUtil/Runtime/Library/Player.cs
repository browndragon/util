using System;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Pubsub;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Library
{
    [Tooltip("Selects between the IPlayables in a library and activates them.")]
    [AddComponentMenu("BDUtil/Library/Player")]
    public class Player : MonoBehaviour, OnState.IEnter, OnState.IExit
    {
        [SerializeField] protected Invokable.Layout buttons;
        [Tooltip("The playable library which this will use.")]
        [SerializeField] protected Library Library;
        public IPlayerLibrary PlayerLibrary => Library.Anycast<IPlayerLibrary>();

        [Tooltip("The 1-centered scale of randomness to for playables (0 none)")]
        [SerializeField] protected float chaos = 1f;
        public float Chaos
        {
            get => chaos * PlayerLibrary.Chaos;
            set => chaos = PlayerLibrary.Chaos == 0 ? value : value / PlayerLibrary.Chaos;
        }
        [Tooltip("The 1-centered scale of power (volume only?) for playables (0 none)")]
        [SerializeField] protected float power = 1f;
        public float Power
        {
            get => power * PlayerLibrary.Power;
            set => power = PlayerLibrary.Power == 0 ? value : value / PlayerLibrary.Power;
        }
        [Tooltip("The 1-centered scale of speed for playables (0 none)")]
        [SerializeField]
        protected float speed = 1f;
        public float Speed
        {
            get => speed * PlayerLibrary.Speed;
            set => speed = PlayerLibrary.Speed == 0 ? value : value / PlayerLibrary.Speed;
        }

        public interface IPlayable
        {
            /// Plays the given asset on the player, returning its duration.
            float PlayOn(Player player);
        }

        [Tooltip("If false, attempts to play while we're already playing are rejected. You can always Force to override.")]
        public bool CanInterrupt = true;

        // Multiple on amount of randomness on produced elements, assuming they support it?
        [Tooltip("The library will be queried for this tag ('' is the default!)")]
        public string Category = "";
        public enum Automation
        {
            None = default,
            Continuous,
            AfterStart,
            OnceOnStart,
        }
        [Tooltip("Whether this should start playing, continue playing after started, etc. See also ")]
        public Automation Automation_;
        public enum Strategies
        {
            [Tooltip("Pick an item from the current category by odds")]
            Random = default,
            [Tooltip("Play the 'next' value in the current category")]
            RoundRobin,
            [Tooltip("Plays the given Category/Index value; you'll need to externally adjust.")]
            UseIndexValue,
        }
        public Strategies Strategy;
        [Tooltip("")]
        public int Index = -1;
        [Tooltip("How long the previous playable indicated it lasted")]
        public Timer Delay = 0f;
        public void PlayByCategoryForce(string tag, bool forceInterrupt)
        {
            if (tag == null) return;
            if (Delay.Tick.IsLive && !CanInterrupt && !forceInterrupt) return;
            Library.ICategory category = Library.GetICategory(tag);
            if (category == null) return;
            Index = Strategy switch
            {
                Strategies.Random => category.GetRandom(),
                Strategies.RoundRobin => (Index + 1).PosMod(category.Count),
                Strategies.UseIndexValue => Index,
                _ => throw Strategy.BadValue(),
            };
            object playable = category[Index.CheckRange(0, category.Count)];
            Delay = playable.Anycast<IPlayable>().PlayOn(this);
            ResetDelay();
            if (Automation_ == Automation.AfterStart) Automation_ = Automation.Continuous;
        }
        void ResetDelay()
        {
            int startWas = Bitcast.Int(Delay.Start);
            Delay.Reset();
            int startIs = Bitcast.Int(Delay.Start);
            if (startIs == startWas) throw new NotSupportedException($"After a reset, {Delay} has same start {startWas} vs {startIs}");
        }
        public void PlayByCategory(string tag) => PlayByCategoryForce(tag, false);
        [Invokable]
        public void PlayCurrentCategory() => PlayByCategory(Category);
        protected void Start()
        {
            switch (Automation_)
            {
                case Automation.OnceOnStart:
                    PlayCurrentCategory();
                    Automation_ = Automation.OnceOnStart;
                    break;
                case Automation.Continuous:
                    ResetDelay();
                    break;
            }
        }
        protected void Update()
        {
            if (Delay.Tick.IsLive) return;
            switch (Automation_)
            {
                case Automation.Continuous: break;
                default: return;
            }
            PlayCurrentCategory();
        }

        void OnState.IEnter.OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => PlayByCategory(Library.GetTagStringFromHash(stateInfo, false));

        void OnState.IExit.OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => PlayByCategory(Library.GetTagStringFromHash(stateInfo, true));

        #region Animator extensions

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
        #endregion  // Animator extensions
    }
}
