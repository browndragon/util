using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil
{
    [Tooltip("Proxies unity monobehaviour events into listenable actions. See TickTopic to turn them into normal topics. See Coroutines to implicitly run on them.")]
    [SuppressMessage("IDE", "IDE0051")]
    public class Ticker : MonoBehaviour
    {
        static event Action onMain;
        public static event Action OnMain
        {
            add
            {
                if (_main == null) onMain += value;
                else value?.Invoke();
            }
            remove => throw new NotSupportedException();
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void _FireDelayed()
        {
            Debug.Log($"FireDelayed subsys reg");
            main.OrThrow();
            onMain?.Invoke();
            onMain = onMain.UnsubscribeAll();
        }

        static Ticker _main;
        [SuppressMessage("IDE", "IDE1006")]
        public static Ticker main
        {
            get => _main ??= FindObjectOfType<Ticker>() ?? Create<Ticker>();
            private set => _main = value;
        }
        public enum Event
        {
            Update = default,
            FixedUpdate,
            LateUpdate,

            Start,
            Enable,
            Disable,
            Destroy,

            ApplicationFocus,
            ApplicationBlur,
            ApplicationPause,
            ApplicationContinue,
            ApplicationQuit,

            DrawGizmos,
            GUI,
            PostRender,
            PreCull,
            PreRender,
        }

        [SerializeField]
        public EnumArray<Event, MemTopic> Topics = new();
        public Ticker()
        {
            for (int i = 0; i < Topics.Data.Length; ++i)
            {
                Topics.Data[i] = new();
            }
        }

        public MemTopic StartEvent => Topics[Event.Start];
        public MemTopic EnableEvent => Topics[Event.Enable];
        public MemTopic DisableEvent => Topics[Event.Disable];
        public MemTopic DestroyEvent => Topics[Event.Destroy];

        public MemTopic FixedUpdateEvent => Topics[Event.FixedUpdate];
        public MemTopic UpdateEvent => Topics[Event.Update];
        public MemTopic LateUpdateEvent => Topics[Event.LateUpdate];

        public MemTopic ApplicationFocusEvent => Topics[Event.ApplicationFocus];
        public MemTopic ApplicationBlurEvent => Topics[Event.ApplicationBlur];
        public MemTopic ApplicationPauseEvent => Topics[Event.ApplicationPause];
        public MemTopic ApplicationContinueEvent => Topics[Event.ApplicationContinue];
        public MemTopic ApplicationQuitEvent => Topics[Event.ApplicationQuit];

        public MemTopic DrawGizmosEvent => Topics[Event.DrawGizmos];
        public MemTopic GUIEvent => Topics[Event.GUI];
        public MemTopic PostRenderEvent => Topics[Event.PostRender];
        public MemTopic PreCullEvent => Topics[Event.PreCull];
        public MemTopic PreRenderEvent => Topics[Event.PreRender];

        #region Lifecycle Events

        private void OnEnable()
        {
            if (_main != null && _main != this)
                Debug.LogWarning($"Somehow you had another ticker {_main.GetInstanceID()} vs {GetInstanceID()}?");
            _main = this;
            EnableEvent.Publish();
        }
        private void OnDisable()
        {
            DisableEvent.Publish();
            if (_main != null && _main != this)
                Debug.LogWarning($"Somehow you had another ticker {_main.GetInstanceID()} vs {GetInstanceID()}?");
            _main = null;
            /// We're hide & don't save, baybee.
            DestroyImmediate(gameObject);
        }
        private void Start() => StartEvent.Publish();
        private void OnDestroy() => DestroyEvent.Publish();

        #endregion

        #region UpdateEvents

        private void FixedUpdate() => FixedUpdateEvent.Publish();
        private void Update() => UpdateEvent.Publish();
        private void LateUpdate() => LateUpdateEvent.Publish();

        #endregion

        #region Application Events

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) ApplicationFocusEvent.Publish();
            else ApplicationBlurEvent.Publish();
        }
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) ApplicationPauseEvent.Publish();
            else ApplicationContinueEvent.Publish();
        }
        private void OnApplicationQuit() => ApplicationQuitEvent.Publish();

        #endregion

        #region Other Events

        private void OnDrawGizmos() => DrawGizmosEvent.Publish();
        private void OnGUI() => GUIEvent.Publish();
        private void OnPostRender() => PostRenderEvent.Publish();
        private void OnPreCull() => PreCullEvent.Publish();
        private void OnPreRender() => PreRenderEvent.Publish();

        #endregion

        public static TTicker Create<TTicker>(string name = "Ticker", HideFlags hideFlags = HideFlags.DontSave) where TTicker : Ticker
        {
            var go = new GameObject(name) { hideFlags = hideFlags };
            DontDestroyOnLoad(go);
            return go.AddComponent<TTicker>();
        }
    }
}