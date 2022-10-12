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
        Randoms.UnitRandom random;
        public Randoms.UnitRandom Random => random ??= random.DistributionPow01(chaos * Library.Chaos);

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

        new public Camera camera { get; private set; }
        new public SpriteRenderer renderer { get; private set; }
        new public AudioSource audio { get; private set; }
        public Transforms.Snapshot transformSnapshot { get; private set; }
        public SpriteRenderers.Snapshot rendererSnapshot { get; private set; }
        public AudioSources.Snapshot audioSnapshot { get; private set; }

        bool HasPlayed;
        protected void OnEnable()
        {
            HasPlayed = false;
            camera = Camera.main;
            renderer = GetComponent<SpriteRenderer>();
            audio = GetComponent<AudioSource>();
            transformSnapshot = transform != null ? transform.GetLocalSnapshot() : default;
            rendererSnapshot = renderer != null ? renderer.GetLocalSnapshot() : default;
            audioSnapshot = audio != null ? audio.GetLocalSnapshot() : default;
        }
        protected void OnDisable()
        {
            transform.SetFromLocalSnapshot(transformSnapshot);
            if (renderer) renderer.SetFromLocalSnapshot(rendererSnapshot);
            if (audio) audio.SetFromLocalSnapshot(audioSnapshot);
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
            Delay = Library.Play(this, category.Entries[Index.CheckRange(0, category.Entries.Count)]);
            Delay.Reset();
            HasPlayed = true;
        }

        private int PickIndex(Library.ICategory category)
        => Strategy switch
        {
            Strategies.Random => category.GetRandom(),
            Strategies.RoundRobin => (Index + 1).PosMod(category.Entries.Count),
            Strategies.UseIndexValue => Index.PosMod(category.Entries.Count),
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
