using System;
using BDUtil.Clone;
using BDUtil.Fluent;
using BDUtil.Math;
using BDUtil.Pubsub;
using BDUtil.Screen;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Library
{
    [Tooltip("Selects between the IPlayables in a library and activates them.")]
    [AddComponentMenu("BDUtil/Library/Player")]
    public class Player : MonoBehaviour, OnState.IEnter, OnState.IExit, Snapshots.IFuzzControls
    {
        [SerializeField] protected Invokable.Layout buttons;
        [Tooltip("The playable library which this will use.")]
        // this would be an "IPlayerLibrary" but you can't use interfaces in the Inspector.
        [SerializeField] public Library Library;

        [Tooltip("The 1-centered scale of randomness to for playables (0 none, .5 half, 2 twice, etc)")]
        [SerializeField] protected float chaos = 1f;
        // Returns a random number centered on .5, which can be used as a random generator elsewhere.
        public float GetChaosRandom() => UnityRandoms.main.Range(.5f - chaos / 2f, .5f + chaos / 2f);
        Randoms.UnitRandom random;
        public Randoms.UnitRandom Random => random ??= GetChaosRandom;

        [Tooltip("The 1-centered scale of power (volume only?) for playables (0 none)")]
        [SerializeField] protected float power = 1f;
        public float Power
        {
            get => power * Library.Power;
            set => power = Library.Power == 0 ? value : value / Library.Power;
        }
        [Tooltip("The 1-centered scale of speed for playables (0 none)")]
        [SerializeField]
        protected float speed = 1f;
        public float Speed
        {
            get => speed * Library.Speed;
            set => speed = Library.Speed == 0 ? value : value / Library.Speed;
        }

        public SpriteRenderer spriteRenderer { get; private set; }
        public AudioSource audioSource { get; private set; }
        Postfab postfab;

        bool HasPlayed;
        protected void OnEnable()
        {
            HasPlayed = false;
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
            postfab = GetComponent<Postfab>();
        }
        protected void OnDisable()
        {
            if (postfab == null || postfab.Link == null) return;
            SpriteRenderer wasRenderer = postfab.Link.GetComponent<SpriteRenderer>();
            if (spriteRenderer && wasRenderer) spriteRenderer.SetFromLocalSnapshot(wasRenderer.GetLocalSnapshot());
            AudioSource wasAudioSource = postfab.Link.GetComponent<AudioSource>();
            if (audioSource && wasAudioSource) audioSource.SetFromLocalSnapshot(wasAudioSource.GetLocalSnapshot());
            transform.SetFromLocalSnapshot(postfab.Link.transform.GetLocalSnapshot());
        }

        [Tooltip("If false, attempts to play while we're already playing are rejected. You can always Force to override.")]
        public bool CanInterrupt = true;

        // Multiple on amount of randomness on produced elements, assuming they support it?
        [Tooltip("The library will be queried for this tag ('' is the default!)")]
        public string Category = "";
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
        public enum Inertias
        {
            None = default,
            BeginInMotion,
            RemainInMotion,
            [Tooltip("Plays _once_ after each OnEnable")]
            OnEnable,
        }
        [Tooltip("Under which circumstances should this autoplay?")]
        public Inertias Inertia;

        [Tooltip("Which index entry within a category was last played (or if UseIndexValue, next played)")]
        public int Index = -1;
        [Tooltip("How long the previous playable indicated it lasted")]
        public Timer Delay = 0f;
        public void PlayByCategoryForce(string tag, bool forceInterrupt)
        {
            if (tag == null) return;
            if (Delay.Tick.IsLive && !CanInterrupt && !forceInterrupt) return;
            Library.ICategory category = Library.GetICategory(tag);
            if (category == null) return;
            Index = PickIndex(category);
            Delay = Library.Play(this, category.Entries[Index.CheckRange(0, category.Count)]);
            Delay.Reset();
            HasPlayed = true;
        }

        private int PickIndex(Library.ICategory category)
        => Strategy switch
        {
            Strategies.Random => category.GetRandom(),
            Strategies.RoundRobin => (Index + 1).PosMod(category.Count),
            Strategies.UseIndexValue => Index.PosMod(category.Count),
            _ => throw Strategy.BadValue(),
        };

        public void PlayByCategory(string tag) => PlayByCategoryForce(tag, false);
        [Invokable]
        public void PlayCurrentCategory() => PlayByCategory(Category);
        protected void Update()
        {
            if (Delay.Tick.IsLive) return;
            switch (Inertia)
            {
                case Inertias.OnEnable:
                    if (HasPlayed) return;
                    goto case Inertias.BeginInMotion;
                case Inertias.RemainInMotion:
                    if (!HasPlayed) return;
                    goto case Inertias.BeginInMotion;
                case Inertias.BeginInMotion:
                    PlayCurrentCategory();
                    break;
                default: return;
            }
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
