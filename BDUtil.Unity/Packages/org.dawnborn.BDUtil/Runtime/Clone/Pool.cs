using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Fluent;
using BDUtil.Raw;
using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BDUtil.Clone
{
    /// Config & display for cloned instances
    public class Pool : StaticAsset<Pool>
    {
        public int CacheSceneLimit = 100;
        public int PerCacheLimit = 10;
        [Flags]
        public enum LogEvents
        {
            None = default,
            Awake = 1 << 0,
            OnDestroyInvalid = 1 << 1,
            OnDestroyValid = 1 << 2,
        }
        public LogEvents LogInstance = LogEvents.Awake;
        public LogEvents LogOthers = Enums<LogEvents>.Everything;

        public string SceneName => $"{name}.Scene";
        public string PreAcquireMessage = "PreAcquire";
        public string PreReleaseMessage = "PreRelease";
        public string PreDestroyMessage = "PreDestroy";
        Scene cachingScene;
        public Scene CachingScene
        {
            get
            {
                if (cachingScene.IsValid()) return cachingScene;
                cachingScene = SceneManager.GetSceneByName(SceneName);
                if (cachingScene.IsValid()) return cachingScene;
                Scene active = SceneManager.GetActiveScene();
                cachingScene = SceneManager.CreateScene(SceneName, new CreateSceneParameters()
                {
                    localPhysicsMode = LocalPhysicsMode.None
                });
                Scene nowActive = SceneManager.GetActiveScene();
                if (active != nowActive)
                {
                    SceneManager.SetActiveScene(active);
                    throw new NotSupportedException($"Didn't expect caching scene to switch {active}=>{nowActive}!");
                }
                return cachingScene;
            }
        }
        [SerializeField, SuppressMessage("IDE", "IDE0044")] StoreMap<GameObject, List<GameObject>> caches = new();
        [SerializeField, SuppressMessage("IDE", "IDE0044")] StoreMap<int, GameObject> extant = new();

        protected void Awake()
        {
            caches.Collection.Clear();
            extant.Collection.Clear();
        }

        void Log(LogEvents @event, Postfab postfab)
        {
            if (!(postfab.IsPostfabInstance && LogInstance.HasFlag(@event) || !postfab.IsPostfabInstance && LogOthers.HasFlag(@event))) return;
            if (@event == LogEvents.OnDestroyInvalid) Debug.LogWarning($"{postfab.FabType}.{@event}: {postfab.IDStr()}.Link={postfab.Link.IDStr()}!");
            else Debug.Log($"{postfab.FabType}.{@event}: {postfab.IDStr()}.Link={postfab.Link.IDStr()}");
        }

        internal void OnNewCloneAwake(Postfab newClone)
        {
            extant.Collection[newClone.gameObject.GetInstanceID()] = newClone.gameObject;
        }
        internal void OnNewCloneDestroyed(Postfab newClone)
        {
            extant.Collection.Remove(newClone.gameObject.GetInstanceID());

            bool isValidRelease = (!newClone.IsPostfabInstance || !newClone.gameObject.scene.IsValid()) && !newClone.IsPrefabAsset;
            Log(isValidRelease ? LogEvents.OnDestroyValid : LogEvents.OnDestroyInvalid, newClone);
        }

        public GameObject Acquire(GameObject prefab, bool activate = true)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            List<GameObject> cache = caches.Collection.GetValueOrDefault(prefab);
            GameObject postfab = null;
            if (cache != null)
            {
                while (cache.Count > 0) if (null != (postfab = cache.PopBack())) break;
                if (cache.Count == 0) caches.Collection.Remove(prefab);
            }
            if (postfab == null) postfab = Postfab.InstantiateInactiveCloneWithRoot(prefab)?.gameObject;
            SceneManager.MoveGameObjectToScene(postfab, SceneManager.GetActiveScene());
            if (!PreAcquireMessage.IsEmpty()) postfab.SendMessage(PreAcquireMessage, SendMessageOptions.DontRequireReceiver);
            if (activate) postfab.SetActive(true);
            return postfab;
        }
        public T Acquire<T>(T prefab, bool activate = true)
        where T : Component
        => Acquire(prefab.gameObject, activate).GetComponent<T>();

        public void Release(GameObject postfab)
        {
            if (postfab == null) throw new ArgumentNullException(nameof(postfab));
            Postfab tag = postfab.GetComponent<Postfab>();
            if (tag == null)
            {  // No tag at all; mundane destroy.
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                Destroy(postfab);
                return;
            }
            if (CachingScene.rootCount >= CacheSceneLimit || PerCacheLimit <= 0)
            {
                /// We don't care what you are, but you must die; we're over-size.
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                tag.SafeDestroy();
                return;
            }
            if (!tag.IsPostfabInstance)
            {
                // Non-postfabs don't cache, so let's go home. Note: we denature postfabs before destroying them, so that this is reentrant.
                // this happens if you destroy oncolliderexit, since exiting the collider re-triggers the destroy...
                return;
            }
            List<GameObject> cache = caches.Collection.GetValueOrDefault(tag.Link);
            if (cache == null) cache = caches.Collection[tag.Link] = new();
            if (cache.Count >= PerCacheLimit)
            {
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                tag.SafeDestroy();
                return;
            }
            // Finally, we're caching. Ahh.
            if (!PreReleaseMessage.IsEmpty()) postfab.SendMessage(PreReleaseMessage, SendMessageOptions.DontRequireReceiver);
            postfab.SetActive(false);
            SceneManager.MoveGameObjectToScene(postfab, CachingScene);
            cache.Add(postfab);

        }
    }
}
