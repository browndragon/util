using UnityEngine;

namespace BDUtil.Clone
{
    using System;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif
    /// A statically loaded scriptable object.
    /// If your singleton needs access to unity lifecycle events, see Ticker.
    /// This isn't always correct; if you want to set a default value, you might prefer to make it an entry in StaticAsset**s**.
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class Cloned : MonoBehaviour
    {
        [field: SerializeField] public GameObject Root { get; internal set; }
        public override string ToString() => $"{base.ToString()}:{gameObject.GetInstanceID()}";

        protected void Awake() => Pool.main.OnNewCloneAwake(this);
        protected void OnDestroy() => Pool.main.OnNewCloneDestroyed(this);
        // Editor only! So this can set the root appropriately for objects already in scene... We Hope.
        protected void OnValidate() => Root = FixCloneRootFromInstance(this);

        /// Knocks the prefab unconscious, instantiates it with link if we have access to the prefab utility,
        /// tags the child, and returns it (and re-awakens the prefab if that's relevant).
        /// Caller has to awaken it!
        internal static Cloned InstantiateInactiveCloneWithRoot(GameObject prefab)
        {
            bool wasActive = prefab.activeSelf;
            GameObject postfab = null;
            try
            {
                if (wasActive) prefab.SetActive(false);
                postfab =
#if UNITY_EDITOR
                    (GameObject)PrefabUtility.InstantiatePrefab(prefab);
#else  // !UNITY_EDITOR
                    UnityEngine.Object.Instantiate(gameObject);
#endif  // UNITY_EDITOR
            }
            finally { if (wasActive) prefab.SetActive(true); }
            if (postfab == null) return null;
            Cloned tag = postfab.GetComponent<Cloned>() ?? postfab.AddComponent<Cloned>();
            tag.Root = prefab;
            return tag;
        }
        // Given a Cloned tag (ie, an instance), determines its clone root using local & editor (if available) information.
        // This is "correct", and so see the Cloned.OnValidate where it's used.
        internal static GameObject FixCloneRootFromInstance(Cloned cloned = default)
        {
#if UNITY_EDITOR
            if (cloned != null)
            {
                bool isPartVariant = PrefabUtility.IsPartOfVariantPrefab(cloned.gameObject);
                bool isAsset = PrefabUtility.IsPartOfNonAssetPrefabInstance(cloned.gameObject);
                if (!isAsset)
                {
                    Debug.Log($"Cloned is an _asset_ somehow?! Ignoring. {cloned.IDStr()}.Root={cloned.Root.IDStr()}");
                    return null;
                }
                PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (stage?.IsPartOfPrefabContents(cloned.gameObject) ?? false)
                {
                    Debug.Log($"Cloned is a prefab stage object, ignoring {cloned.IDStr()}.Root={cloned.Root.IDStr()}");
                    return null;
                }
                GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(cloned.gameObject);
                if (prefabRoot?.scene.IsValid() ?? false)
                {
                    throw new NotSupportedException($"Cloned points to a 'prefab root' in active scene `{prefabRoot?.scene.path}`: {cloned.IDStr()}.Root={cloned.Root.IDStr()} vs {prefabRoot.IDStr()}");
                }
                switch ((prefabRoot != null, cloned.Root != null))
                {
                    case (false, false): return null;
                    case (true, false): return prefabRoot;
                    case (false, true):
                        if (!Application.IsPlaying(cloned.gameObject)) return null;
                        throw new NotSupportedException($"You must remove Cloned from non-prefab {cloned.IDStr()}.Root={cloned.Root.IDStr()}!");
                    case (true, true):
                        if (prefabRoot == cloned.Root) return prefabRoot;
                        else if (!Application.IsPlaying(cloned.gameObject))
                        {
                            Debug.LogWarning($"Clone on prefab stage (?) {cloned.IDStr()}.Root={cloned.Root.IDStr()}!={prefabRoot.IDStr()}");
                            return prefabRoot;
                        }
                        else if (prefabRoot == cloned.gameObject) throw new NotSupportedException($"Impossible! Cloned is its own clone {cloned.IDStr()}.Root=<self>");
                        else throw new NotSupportedException($"Impossible! Cloned & prefab have different roots: {cloned.IDStr()}.Root={cloned.Root.IDStr()} != {prefabRoot.IDStr()}");
                }
            }
#endif  // UNITY_EDITOR
            return cloned?.Root;
        }
    }
}
