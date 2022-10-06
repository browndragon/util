using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Fluent;
using BDUtil.Raw;
using BDUtil.Screen;
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
            Postfab pretag = prefab.GetComponent<Postfab>();
            Postfab posttag;

            if (postfab == null)
            {
                posttag = Postfab.InstantiateInactiveCloneWithRoot(
                    pretag?.Asset ?? prefab, prefab, pretag?.ChildFabType ?? Postfab.FabTypes.Unknown
                );
                postfab = posttag.gameObject;
            }
            else posttag = postfab.GetComponent<Postfab>().OrThrow();

            // TODO: should we actually be doing this? It's probably pretty expensive...
            SceneManager.MoveGameObjectToScene(postfab, SceneManager.GetActiveScene());
            posttag.PreAcquire();
            if (activate) postfab.SetActive(true);
            return postfab;
        }
        public T Acquire<T>(T prefab, bool activate = true)
        where T : Component
        => Acquire(prefab.gameObject, activate).GetComponent<T>();

        // Okay, weird but bear with me.
        // You have a prefab of a level chunk consisting of postfab.ActuallyAPrefabs.
        // You want to instantiate it using caching.
        // This method does that: you feed it the prefab, it iterates the top level children of the prefab,
        // it gets the postfab.Asset sitting behind each one, it instantiates it.
        // Each one's top level object gets called with the PreAcquireMessage with the match from the model, so you can apply modifications
        // to the prefab and have them copied to the postfab (even with caching) if your script catches that message.
        // Transforms are done for you automatically, though.
        // RelativeTo lets you adjust the position of the newly created object -- remember, they'll be dumped at top level! --
        public void AcquireViaAssetsOfChildren(Transform prefab, List<GameObject> clones, Transforms.Local relativeTo = default, bool awake = true)
        {
            foreach (Component c in prefab.gameObject.GetComponents<Component>())
            {
                if (c is not Transform) throw new ArgumentException($"Can't break open {prefab?.IDStr()}; it has at least {c} which won't get copied");
            }
            foreach (Transform child in prefab.GetChildren())
            {
                Postfab targetPostfab = child.GetComponent<Postfab>();
                GameObject model = targetPostfab?.Asset ?? child.gameObject;
                GameObject clone = Acquire(model, false);
                Transforms.Local snapshot = child.GetLocalSnapshot();
                snapshot.ContextualizeUnder(relativeTo);
                clone.transform.SetFromLocalSnapshot(snapshot);
                if (awake) clone.SetActive(true);
                clones?.Add(clone);
            }
        }

        public void Release(GameObject postfab)
        {
            Debug.Log($"Attempt release {postfab}", postfab);
            if (postfab == null) throw new ArgumentNullException(nameof(postfab));
            Postfab tag = postfab.GetComponent<Postfab>();
            if (tag == null)
            {  // No tag at all; mundane destroy.
                Debug.Log($"No tag {postfab}", postfab);
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                Destroy(postfab);
                return;
            }
            if (CachingScene.rootCount >= CacheSceneLimit || PerCacheLimit <= 0)
            {
                Debug.Log($"Too many, destroying {postfab}", postfab);
                /// We don't care what you are, but you must die; we're over-size.
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                tag.SafeDestroy();
                return;
            }
            if (!tag.IsPostfabInstance)
            {
                Debug.Log($"Not a _real_ postfab {postfab}", postfab);
                // Non-postfabs don't cache, so let's go home. Note: we denature postfabs before destroying them, so that this is reentrant.
                // this happens if you destroy oncolliderexit, since exiting the collider re-triggers the destroy...
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                tag.SafeDestroy();
                return;
            }
            List<GameObject> cache = caches.Collection.GetValueOrDefault(tag.Link);
            if (cache == null) cache = caches.Collection[tag.Link] = new();
            if (cache.Count >= PerCacheLimit)
            {
                Debug.Log($"Too many in shared cache {postfab}", postfab);
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                tag.SafeDestroy();
                return;
            }
            Debug.Log($"Keeping pooled copy {postfab}", postfab);
            // Finally, we're caching. Ahh.
            tag.PreRelease();
            postfab.SetActive(false);
            SceneManager.MoveGameObjectToScene(postfab, CachingScene);
            cache.Add(postfab);
        }
    }
}
