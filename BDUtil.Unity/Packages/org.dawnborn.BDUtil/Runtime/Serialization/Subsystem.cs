using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// This is a hack for scriptableobjects et al, which need to push initialization logic from "early" into "after subsystem intialization".
    /// For instance, you might want to ensure it's late enough to create singleton monobehaviours, that any scene is active, etc.
    public class Subsystem
    {
        /// Bridge from scriptableobjects -- initialized BEFORE the scene is ready -- to scene objects -- initialized after.
        /// This is run on subsystemregistration.
        [SuppressMessage("IDE", "IDE1006")]
        static event Action onReady;
        static bool _initialized;
        public static event Action OnReady
        {
            add
            {
                if (!_initialized) onReady += value;
                else value?.Invoke();
            }
            remove => throw new NotSupportedException();
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnSubsystemRegistrationFireDelayed()
        {
            _initialized = true;
            Action initializing = onReady;
            onReady = null;
            initializing?.Invoke();
            initializing.UnsubscribeAll();
        }
    }
}