using UnityEditor;
using UnityEditor.SceneManagement;

namespace BDUtil.Clone.Editor
{
    [CustomEditor(typeof(Postfab))]
    public class PostfabEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                EditorGUILayout.HelpBox($"All Postfab tracking suspended; Prefab stage editing {stage.assetPath}", MessageType.Warning);
            }
            DrawDefaultInspector();
        }
    }
}