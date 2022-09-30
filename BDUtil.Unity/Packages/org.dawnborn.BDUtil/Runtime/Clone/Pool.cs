using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        string SceneName => $"{name}.Scene";
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
            newClone.HasRoot.OrThrow();
            extant.Collection[newClone.gameObject.GetInstanceID()] = newClone.gameObject;
        }
        internal void OnNewCloneDestroyed(Cloned newClone)
        {
            extant.Collection.Remove(newClone.gameObject.GetInstanceID());
            if (!newClone.HasRoot) return;  // This is fine; it's been denatured before destruction.
            // THIS is fine; the scene is going away. It should already have deactivated
            if (!newClone.gameObject.scene.IsValid()) return;
        }

        public GameObject Acquire(GameObject prefab, bool activate = true)
        {
            List<GameObject> cache = caches.Collection.GetValueOrDefault(prefab);
            GameObject postfab = null;
            if (cache != null)
            {
                while (cache.Count > 0) if (null != (postfab = cache.PopBack())) break;
                if (cache.Count == 0) caches.Collection.Remove(prefab);
            }

            if (postfab == null)
            {  // We need to create a new one. That's fine!
                bool wasActive = prefab.activeSelf;
                if (wasActive) prefab.SetActive(false);
                postfab = EditorUtils.InstantiateWithLink(prefab);
                Cloned tag = postfab.GetComponent<Cloned>() ?? postfab.AddComponent<Cloned>();
                // This is the really _really_ important step!
                // It ensures that the clonetag points to the root, instead of getting re-mapped to self!
                tag.Root = prefab;
                if (wasActive) prefab.SetActive(true);
            }

            SceneManager.MoveGameObjectToScene(postfab, SceneManager.GetActiveScene());
            if (activate) postfab.SetActive(true);
            return postfab;
        }
        public T Acquire<T>(T prefab, bool activate = true)
        where T : Component
        => Acquire(prefab.gameObject, activate).GetComponent<T>();

        public void Release(GameObject postfab)
        {
            Cloned tag = postfab.GetComponent<Cloned>().OrThrow();
            if (CachingScene.rootCount >= CacheSceneLimit || PerCacheLimit <= 0)
            {
                tag.Root = null;
                Destroy(postfab);
                return;
            }

            List<GameObject> cache = caches.Collection.GetValueOrDefault(tag.Root);
            if (cache == null) cache = caches.Collection[tag.Root] = new();
            if (cache.Count >= PerCacheLimit)
            {
                tag.Root = null;
                Destroy(postfab);
                return;
            }
            postfab.SetActive(false);
            SceneManager.MoveGameObjectToScene(postfab, CachingScene);
            cache.Add(postfab);
        }
    }
}
