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
    /// Config & display for cloned instances.
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
        public LogEvents LogEvent = LogEvents.Awake;

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
            if (!LogEvent.HasFlag(@event)) return;
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

            bool isValidRelease = newClone.FabType switch
            {
                Postfab.FabTypes.Unknown => true,
                Postfab.FabTypes.PostfabPostmortem => true,
                Postfab.FabTypes.Postfab => !newClone.gameObject.scene.IsValid(),
                _ => false,
            };
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
            if (postfab == null)
            {
                Postfab pretag = prefab.GetComponent<Postfab>();
                GameObject asset = pretag != null ? pretag.Asset : prefab;
                Postfab.FabTypes fabType = pretag != null ? pretag.ChildFabType : Postfab.FabTypes.Unknown;
                Postfab posttag = Postfab.InstantiateInactiveCloneWithRoot(asset, prefab, fabType);
                postfab = posttag.gameObject;
            }
            Transforms.Snapshot snapshot = new(prefab.transform);
            snapshot.ApplyTo(postfab.transform);
            // TODO: should we actually be doing this? It's probably pretty expensive...
            SceneManager.MoveGameObjectToScene(postfab, SceneManager.GetActiveScene());
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
        public void AcquireViaAssetsOfChildren(Transform prefab, List<GameObject> clones, bool awake = true)
        {
            foreach (Component c in prefab.gameObject.GetComponents<Component>())
            {
                if (c is not Transform) throw new ArgumentException($"Can't break open {prefab?.IDStr()}; it has at least {c} which won't get copied");
            }
            // Transforms.Snapshot snapshot = new(prefab.transform);
            // Matrix4x4 parent = snapshot.Matrix;
            foreach (Transform child in prefab.GetChildren())
            {
                Postfab targetPostfab = child.GetComponent<Postfab>();
                GameObject model = targetPostfab != null ? targetPostfab.Asset : child.gameObject;
                GameObject clone = Acquire(model, false);
                Transforms.Snapshot childSnapshot = new(child.transform);
                // childSnapshot = parent * childSnapshot.Matrix;
                childSnapshot.ApplyTo(clone.transform);
                if (awake) clone.SetActive(true);
                clones?.Add(clone);
            }
        }

        public void Release(GameObject postfab)
        {
            if (postfab == null) throw new ArgumentNullException(nameof(postfab));
            Postfab tag = postfab.GetComponent<Postfab>();
            if (tag == null)
            {  // No tag at all; mundane destroy.
                Debug.Log($"No tag {postfab}", postfab);
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                Destroy(postfab);
                return;
            }
            switch (tag.FabType)
            {
                case Postfab.FabTypes.Postfab: break;
                case Postfab.FabTypes.PostfabPostmortem: return;  // Some kind of reentrant... Just let it go. It's dead, jim.
                case Postfab.FabTypes.ActuallyAPrefab:  // Fallthrough
                case Postfab.FabTypes.Demoted:
                    throw new NotSupportedException($"{tag.IDStr()} is nondestructible!!!");
                case Postfab.FabTypes.Unknown:
                    Debug.Log($"Not a _real_ postfab {tag.IDStr()} of {tag.Link}/{tag.Asset}", postfab);
                    // Non-postfabs don't cache, so let's go home. Note: we denature postfabs before destroying them, so that this is reentrant.
                    // this happens if you destroy oncolliderexit, since exiting the collider re-triggers the destroy...
                    if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                    Destroy(tag.gameObject);
                    return;
            }
            if (CachingScene.rootCount >= CacheSceneLimit || PerCacheLimit <= 0)
            {
                /// We don't care what you are, but you must die; we're over-size.
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                tag.SafeDestroy();
                return;
            }
            List<GameObject> cache = caches.Collection.GetValueOrDefault(tag.Link) ?? (caches.Collection[tag.Link] = new());
            if (cache.Count >= PerCacheLimit)
            {
                if (!PreDestroyMessage.IsEmpty()) postfab.SendMessage(PreDestroyMessage, SendMessageOptions.DontRequireReceiver);
                tag.SafeDestroy();
                return;
            }
            postfab.SetActive(false);
            SceneManager.MoveGameObjectToScene(postfab, CachingScene);
            cache.Add(postfab);
        }
    }
}
