using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    /// In the editor only; causes prefab instances to maintain their self-links.
    /// There's some slightly weird behaviour, where this leaves the original in the scene unlinked from the child... Ah Well.
    [InitializeOnLoad]
    internal static class CloneLinker
    {
        static CloneLinker() => PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
        static void OnPrefabInstanceUpdate(GameObject instance)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            Clone clone = prefab?.GetComponent<Clone>();
            if (clone == null) return;
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            if (prefabPath.IsEmpty()) return;
            Debug.Log($"OnPrefabInstanceUpdate: {Event.current} @ {prefab}::{instance}", instance);
            Undo.RecordObject(clone, "Setting Clone.PrefabRef");
            clone.PrefabRef = prefabPath;
            AssetDatabase.SaveAssetIfDirty(prefab);
        }
    }
}