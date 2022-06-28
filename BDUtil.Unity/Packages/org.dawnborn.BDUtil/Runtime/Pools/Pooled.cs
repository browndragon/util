using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil
{
    [AddComponentMenu("BTUtil/Pooled")]
    [Tooltip("A pooled game object visits all registered pools at relevant times.")]
    public class Pooled : MonoBehaviour
    {
        [SerializeField, SuppressMessage("IDE", "IDE0044")]
        List<BasePool> Pools = new();

        [SerializeField, SuppressMessage("IDE", "IDE0044"), SuppressMessage("IDE", "IDE0051")]
        [Tooltip("Creates a controller-owned pool with the given name")]
        string ControllerPool;
        [SerializeField, SuppressMessage("IDE", "IDE0044"), SuppressMessage("IDE", "IDE0051")]
        [Tooltip("Creates a controller-owned registry with the given name")]
        string ControllerRegistry;

        [SuppressMessage("IDE", "IDE0051")]
        void Awake()
        {
            if (ControllerPool?.Length > 0)
            {
                Pools.Add(GameControllers.GetOrPut<Pool>(ControllerPool));
            }
            if (ControllerRegistry?.Length > 0)
            {
                Pools.Add(GameControllers.GetOrPut<Registry>(ControllerRegistry));
            }
        }

        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable() { foreach (BasePool pool in Pools) pool.EnableMe(gameObject); }
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() { foreach (BasePool pool in Pools) pool.DisableMe(gameObject); }
    }
}