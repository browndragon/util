using BDUtil.Math;
using BDUtil.Play;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Library
{
    [Tooltip("Selects between the IPlayables in a library and activates them.")]
    [AddComponentMenu("BDUtil/Library/Player")]
    public class Player : MonoBehaviour, OnState.IEnter, OnState.IExit
    {
        public interface IPlayerLibrary
        {
            void Validate(Player player);
            // void Enable(Player player);
            // void Disable(Player player);
            bool Play(Player player);
        }
        [SerializeField] protected Invokable.Layout buttons;
        [Tooltip("The playable library which this will use.")]
        // this would be an "IPlayerLibrary" but you can't use interfaces in the Inspector.
        [Expandable] public Library Library;
        public IPlayerLibrary PlayerLibrary => (IPlayerLibrary)Library;
        protected void OnValidate()
        {
            if (Library is IPlayerLibrary playerLibrary)
            {
                playerLibrary.Validate(this);
                return;
            }
            Debug.LogWarning($"Player {this} has non-player-library {Library}!");
        }
        public Library.EntryChooser Chooser;

        bool HasPlayed;
        protected void OnEnable()
        {
            // PlayerLibrary.Enable(this);
            HasPlayed = false;
        }
        // protected void OnDisable()
        // {
        //     PlayerLibrary.Disable(this);
        // }
        [Tooltip("If false, attempts to play while we're already playing are rejected. You can always Force to override.")]
        public bool CanInterrupt = true;

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


        [Tooltip("How long the previous playable indicated it lasted")]
        public Delay Delay = 0f;

        public void PlayByCategoryForce(string tag, bool forceInterrupt)
        {
            if (tag == null) return;
            Chooser.Category = tag;
            if (Delay && !CanInterrupt && !forceInterrupt) return;
            HasPlayed |= PlayerLibrary.Play(this);
        }
        public void PlayByCategory(string tag) => PlayByCategoryForce(tag, false);
        [Invokable]
        public void PlayCurrentCategory() => PlayByCategory(Chooser.Category);
        protected void Update()
        {
            if (Delay) return;
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
