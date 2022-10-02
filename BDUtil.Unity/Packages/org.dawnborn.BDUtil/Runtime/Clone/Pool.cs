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
        public LogEvents LogNormal = LogEvents.Awake;
        public LogEvents LogPrefabStage = LogEvents.Awake;

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

        internal void OnNewCloneAwake(Cloned newClone)
        {
            if (!Application.IsPlaying(newClone))
            {
                if (LogPrefabStage.HasFlag(LogEvents.Awake)) Debug.Log($"Clone.PrefabStage.Awake: {newClone.IDStr()}.Root={newClone.Root.IDStr()}");
                return;
            }
            if (LogNormal.HasFlag(LogEvents.Awake)) Debug.Log($"Clone.Awake: {newClone.IDStr()}.Root={newClone.Root.IDStr()}");
            newClone.Root.OrThrow();
            extant.Collection[newClone.gameObject.GetInstanceID()] = newClone.gameObject;
        }
        internal void OnNewCloneDestroyed(Cloned newClone)
        {
            if (newClone == null) return;

            bool isValidRelease = newClone.Root == null || !newClone.gameObject.scene.IsValid();
            bool isPrefabStage = Application.IsPlaying(newClone);
            LogEvents perStage = isPrefabStage ? LogPrefabStage : LogNormal;

            extant.Collection.Remove(newClone.gameObject.GetInstanceID());

            if (isValidRelease)
            {
                if (!perStage.HasFlag(LogEvents.OnDestroyValid)) return;
                Debug.Log(
                    $"Clone{(isPrefabStage ? ".PrefabStage" : "")}.OnDestroy.Valid: {newClone.IDStr()}.Root={newClone.Root.IDStr()}"
                );
                return;
            }
            if (!perStage.HasFlag(LogEvents.OnDestroyInvalid)) return;
            Debug.LogWarning(
                $"Clone{(isPrefabStage ? ".PrefabStage" : "")}.OnDestroy.INVALID: {newClone.IDStr()}.Root={newClone.Root.IDStr()} should be Should Clone.main.Release()ed, not GameObject.Destroy()ed!"
            );
        }

        public GameObject GetPrefab(GameObject postfab)
        {
            if (postfab == null) return null;
            if (!postfab.scene.IsValid()) return postfab;
            Cloned clone = postfab.GetComponent<Cloned>();
            if (clone == null) return null;
            if (clone.Root == null) return null;
            return clone.Root;
        }

        public GameObject Acquire(GameObject prefab, bool activate = true)
        {
            prefab = GetPrefab(prefab);
            List<GameObject> cache = caches.Collection.GetValueOrDefault(prefab);
            GameObject postfab = null;
            if (cache != null)
            {
                while (cache.Count > 0) if (null != (postfab = cache.PopBack())) break;
                if (cache.Count == 0) caches.Collection.Remove(prefab);
            }

            if (postfab == null) postfab = Cloned.InstantiateInactiveCloneWithRoot(prefab)?.gameObject;
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
            Cloned tag = postfab.GetComponent<Cloned>();
            if (tag == null)
            {
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                Destroy(postfab);
                return;
            }

            if (CachingScene.rootCount >= CacheSceneLimit || PerCacheLimit <= 0)
            {
                tag.Root = null;
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                Destroy(postfab);
                return;
            }

            List<GameObject> cache = caches.Collection.GetValueOrDefault(tag.Root);
            if (cache == null) cache = caches.Collection[tag.Root] = new();
            if (cache.Count >= PerCacheLimit)
            {
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                tag.Root = null;
                Destroy(postfab);
                return;
            }
            if (!PreReleaseMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
            postfab.SetActive(false);
            SceneManager.MoveGameObjectToScene(postfab, CachingScene);
            cache.Add(postfab);
        }
    }
}
