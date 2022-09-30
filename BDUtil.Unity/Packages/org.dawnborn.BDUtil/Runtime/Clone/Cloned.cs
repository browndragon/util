using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Clone
{
    /// A statically loaded scriptable object.
    /// If your singleton needs access to unity lifecycle events, see Ticker.
    /// This isn't always correct; if you want to set a default value, you might prefer to make it an entry in StaticAsset**s**.
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Cloned : MonoBehaviour, EditorUtils.ICloned
    {
        [field: SerializeField] public GameObject Root { get; internal set; }
        public bool HasRoot => Root != null;
        public bool WithRoot(out GameObject root) => HasRoot.Let(root = Root);
        public override string ToString() => $"{base.ToString()}:{gameObject.GetInstanceID()}";
        protected void Awake()
        {
            Root = EditorUtils.GetCloneRoot(this);
            Pool.main.OnNewCloneAwake(this);
        }
        protected void OnDestroy() => Pool.main.OnNewCloneDestroyed(this);

        GameObject EditorUtils.ICloned.gameObject => gameObject;
    }
}
