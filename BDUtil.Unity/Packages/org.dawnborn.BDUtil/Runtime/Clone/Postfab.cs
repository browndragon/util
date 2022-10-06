using UnityEngine;

namespace BDUtil.Clone
{
    using System;
    using BDUtil.Fluent;
    using BDUtil.Screen;
    using BDUtil.Serialization;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif
    /// A runtime-aware prefab<->instance ("postfab") tracker. It has *rules*.
    ///
    /// If you `GameObject.Instantiate()` or `.Destroy()` a prefab with a Postfab component at runtime, it will warn you.
    /// Try not to do that.
    /// Instead, `Pool.main.Acquire()` and `.Release()` your Postfab-aware prefabs & postfabs to get automatic caching.
    /// This will emulate the build-time `OnValidate()` tracking of the `prefab` on create, and recycle & suppress warnings OnDestroy.
    /// As a convenience, when you Acquire a non-prefab it will still link to its parent; when you Release a non-postfab it will just destroy.
    /// It's okay if scene destruction `Destroy()`s instead of `Release()`ing postfabs, though of course they won't be recycled!
    ///
    /// During editor `OnValidate()`, this examines editor state to remember the `prefab` that created it.
    /// This persists through to runtime, allowing you to treat objects in the scene from buildtime equivalently to those later created.
    /// Later, during the Postfab Awake and OnDestroy events, it will maintain & collate runtime information through `Pool.main`.
    ///
    /// Postfabs with parents: Try Hard Not To And Definitely Separate Them Before Lifecycle Events Oh God.
    /// In the following code:
    /// ```
    /// public class Example : MonoBehaviour {
    ///   GameObject preA, preB;  // Such that preA is a prefab with a single child, preB
    ///   void Start() {
    ///     GameObject postA = Pool.main.Acquire(preA),postB=postA.transform.GetChild(0).gameObject;
    ///   }
    /// }
    /// ```
    /// preA & postA are simple; they're top-level prefab & postfab and the below caveats don't apply.
    /// But preB & postB (hence: preB & postB) are not simple.
    /// 1) postB's Postfab.prefab is technically _not_ preB.
    ///    Because it was instantiated as a side effect of instantiating preA, it points at `preA.transform.GetChild(0)`,
    ///    which is inside of file preA and not inside of file preB, and so they're technically different.
    /// 2) postB doesn't control its own lifetime.
    ///    When you acquire, release, or destroy preA<->postA, postB is just along for the ride; you can't `Acquire` or `Release` it.
    /// If preA does not have the Postfab component, then
    [DisallowMultipleComponent]
    public sealed class Postfab : MonoBehaviour
    {
        public enum FabTypes
        {
            [Tooltip("A complex/unsupported case. Common for runtime-promoted objects of non-ActuallyAPrefab (like Demoted esp.)")]
            Unknown = default,
            [Tooltip("Common case: an instance in a scene of an ActuallyAPrefab prefab asset")]
            Postfab,
            [Tooltip("Common case: a prefab/variant asset that is root in its file.")]
            ActuallyAPrefab,
            [Tooltip("A postfab which is being destroyed in a controlled fashion")]
            PostfabPostmortem,
            [Tooltip("A prefab or variant asset that is NOT root in its file; cannot produce Prefabs! Also assigned to some instances of those.")]
            Demoted,
        }
        /// The relationship between this object and its prefab (if any).
        [field: SerializeField] public FabTypes FabType { get; private set; }
        /// PrefabAssets might or might not have a link (if they do, they're a variant of it); variants will.
        /// They _will_ have an Asset, and it will be themselves.
        public bool IsPrefabAsset => FabType == FabTypes.ActuallyAPrefab;
        /// Postfabs MUST have a link, and it MUST be a prefab/variant asset file, and it MUST be the same as Asset.
        public bool IsPostfabInstance => FabType == FabTypes.Postfab;

        /// A link to this thing's Instantiating object; basically a prefab, but it might get complicated.
        /// For an instance of a prefab or variant asset, it's the asset.
        /// For an instance of a child of a prefab or variant asset, it's also that child -- which isn't itself the root asset, be warned!
        /// For an "actuallyaprefab"; for a variant it will be the base, for the base, will be null.
        /// For a "postfab", it will be the same as asset: the actuallyaprefab.
        /// For others, it's whatever instantiated them...
        [field: SerializeField] public GameObject Link { get; private set; }
        /// Recursively unfolds Link to find a root asset (or null).
        /// So in the above case where the Link was the child, it would keep following links until it found an Asset link (or null if none).
        /// For an "actuallyaprefab", it's itself.
        /// For a "postfab", it's the same as link: the actuallyaprefab.
        /// For others, it's null (even if there is an asset on disk).
        [field: SerializeField] public GameObject Asset { get; private set; }
        // Editor only! So this can set the root appropriately for objects already in scene... We Hope.
        internal void OnValidate() => CopyPrefabUtilityIntoInstance(this);
        internal void Awake() => Pool.main.OnNewCloneAwake(this);
        internal void OnDestroy() => Pool.main.OnNewCloneDestroyed(this);
        internal void SafeDestroy()
        {
            if (FabType == FabTypes.Postfab) FabType = FabTypes.PostfabPostmortem;
            Destroy(gameObject);
        }

        /// Knocks the prefab unconscious, instantiates it with link if we have access to the prefab utility,
        /// tags the child, and returns it (and re-awakens the prefab if that's relevant).
        /// Caller has to awaken it!
        internal static Postfab InstantiateInactiveCloneWithRoot(GameObject prefab, GameObject parent, FabTypes fabType)
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
            Postfab tag = postfab.GetComponent<Postfab>() ?? postfab.AddComponent<Postfab>();
            tag.FabType = fabType;
            tag.Link = parent;
            tag.Asset = prefab;
            return tag;
        }
        internal FabTypes ChildFabType
        {
            get => FabType switch
            {
                FabTypes.ActuallyAPrefab => FabTypes.Postfab,
                FabTypes.Demoted => FabTypes.Unknown,  // Fine, just turns of precaching.
                FabTypes.Unknown => FabTypes.Unknown,  // Runtime clone. Weird but fine.
                _ => FabTypes.Unknown,
            };
        }

        // Given a Cloned tag (ie, an instance), determines its clone root using local & editor (if available) information.
        // This is "correct", and so see the Cloned.OnValidate where it's used.
        internal static void CopyPrefabUtilityIntoInstance(Postfab cloned)
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // If we're playing, the PrefabUtility's answers are insane.
                // So don't change anything; it wouldn't _really_ matter because anything added now will get tossed when we stop playing.
                return;
            }
            // Same idea; if the prefab stage is open, the data we get is kind of crazy.
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                // It would be ideal to limit this to just the bad instance, but you can't:
                // this is called onvalidate ("before awake") and the prefabstageutility throws an exception.
                return;
            }
            if (!PrefabUtility.IsPartOfAnyPrefab(cloned.gameObject))
            {
                // Either it's not a prefab -- naughty, having the Postfab component -- or despite EditorApplication up there,
                // it's LYING and we're actually at runtime.
                // Which... god, shoot me.
                Debug.Log($"{cloned.IDStr()} isn't a prefab but wants tracking; can't, might destroy the fabric of spacetime");
                return;
            }
            GameObject hadLink = cloned.Link, hadAsset = cloned.Asset;

            cloned.Link = PrefabUtility.GetCorrespondingObjectFromSource(cloned.gameObject);
            Postfab prefab = cloned.Link?.GetComponent<Postfab>();
            cloned.Asset = prefab?.Asset;
            cloned.FabType = FabTypes.Unknown;

            if (PrefabUtility.IsPartOfPrefabAsset(cloned.gameObject))
            {
                if (cloned.transform.parent == null)
                {  // A prefab asset with no parents: a prefab afaiac.
                    Debug.Log($"{PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(cloned)} contains {cloned.IDStr()}");
                    cloned.FabType = FabTypes.ActuallyAPrefab;
                    // Special rule, we need to set ourselves as the asset type.
                    cloned.Asset = cloned.gameObject.OrThrow();
                    return;
                }
                // Oh sure, we _were_ an "actually a prefab", but not parent in prefab means not really a prefab.
                // Baleeted.
                cloned.FabType = FabTypes.Demoted;
                return;
            }
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(cloned.gameObject))
            {
                if (
                    hadLink != null && cloned.Link != null && hadLink != cloned.Link
                ) Debug.Log($"Updating {cloned.IDStr()}.link: was {hadLink.IDStr()} -> now {cloned.Link.IDStr()}");
                if (prefab?.IsPrefabAsset ?? false)
                {  // Only if our parents were a prefab asset should we become a postfab.
                    cloned.FabType = FabTypes.Postfab;
                    cloned.Asset = prefab.Asset.OrThrow();
                    return;
                }
            }
            Debug.Log($"Complex {cloned.IDStr()} link undiagnosed.");
            return;

#endif  // UNITY_EDITOR
            throw new NotImplementedException($"This should not be callable at runtime.");
        }
    }
}
