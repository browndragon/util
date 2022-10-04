using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BDUtil.Clone.Editor
{
    public class PrefabWindow : EditorWindow
    {
        [MenuItem("Window/BDUtil/Prefab")]
        static protected void ShowToolbar()
        {
            GetWindow<PrefabWindow>("PrefabWindow");
        }

        private Vector2 scroll;
        public void OnGUI()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                EditorGUILayout.HelpBox("Select a prefab or instance to examine.", MessageType.Info);
                return;
            }

            EditorGUIUtility.labelWidth = 300;
            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                EditorGUILayout.ObjectField("Selection", go, typeof(GameObject), allowSceneObjects: true);
                EditorGUILayout.LabelField("ID", go.GetInstanceID().ToString());

                EditorGUILayout.Space();
                Postfab cloned = go.GetComponent<Postfab>();
                if (cloned != null)
                {
                    EditorGUILayout.LabelField("Postfab", EditorStyles.boldLabel);

                    EditorGUILayout.ObjectField("Link", cloned.Link, typeof(GameObject), allowSceneObjects: true);
                    EditorGUILayout.LabelField("Link.FabType", cloned.FabType.ToString());
                    EditorGUILayout.LabelField("Link.ID", cloned.Link?.GetInstanceID().ToString() ?? "");
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField("PrefabStageUtility", EditorStyles.boldLabel);

                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                EditorGUILayout.LabelField("GetCurrentPrefabStage", stage != null ? "has stage" : "null");
                if (stage != null)
                {
                    EditorGUILayout.LabelField("IsPartOfPrefabContents", "" + stage.IsPartOfPrefabContents(go));
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("PrefabUtility", EditorStyles.boldLabel);

                EditorGUILayout.LabelField("IsAddedComponentOverride", "" + PrefabUtility.IsAddedComponentOverride(go));
                EditorGUILayout.LabelField("IsAddedGameObjectOverride", "" + PrefabUtility.IsAddedGameObjectOverride(go));
                EditorGUILayout.LabelField("IsAnyPrefabInstanceRoot", "" + PrefabUtility.IsAnyPrefabInstanceRoot(go));
                EditorGUILayout.LabelField("IsDisconnectedFromPrefabAsset", "" + PrefabUtility.IsDisconnectedFromPrefabAsset(go));
                EditorGUILayout.LabelField("IsOutermostPrefabInstanceRoot", "" + PrefabUtility.IsOutermostPrefabInstanceRoot(go));
                EditorGUILayout.LabelField("IsPartOfAnyPrefab", "" + PrefabUtility.IsPartOfAnyPrefab(go));
                EditorGUILayout.LabelField("IsPartOfImmutablePrefab", "" + PrefabUtility.IsPartOfImmutablePrefab(go));
                EditorGUILayout.LabelField("IsPartOfModelPrefab", "" + PrefabUtility.IsPartOfModelPrefab(go));
                EditorGUILayout.LabelField("IsPartOfNonAssetPrefabInstance", "" + PrefabUtility.IsPartOfNonAssetPrefabInstance(go));
                EditorGUILayout.LabelField("IsPartOfPrefabAsset", "" + PrefabUtility.IsPartOfPrefabAsset(go));
                EditorGUILayout.LabelField("IsPartOfPrefabInstance", "" + PrefabUtility.IsPartOfPrefabInstance(go));
                EditorGUILayout.LabelField("IsPartOfPrefabThatCanBeAppliedTo", "" + PrefabUtility.IsPartOfPrefabThatCanBeAppliedTo(go));
                EditorGUILayout.LabelField("IsPartOfRegularPrefab", "" + PrefabUtility.IsPartOfRegularPrefab(go));
                EditorGUILayout.LabelField("IsPartOfVariantPrefab", "" + PrefabUtility.IsPartOfVariantPrefab(go));
                EditorGUILayout.LabelField("IsPrefabAssetMissing", "" + PrefabUtility.IsPrefabAssetMissing(go));

                EditorGUILayout.ObjectField("GetCorrespondingObjectFromOriginalSource", PrefabUtility.GetCorrespondingObjectFromOriginalSource(go), typeof(GameObject), allowSceneObjects: true);
                EditorGUILayout.ObjectField("GetCorrespondingObjectFromSource", PrefabUtility.GetCorrespondingObjectFromSource(go), typeof(GameObject), allowSceneObjects: true);
                EditorGUILayout.ObjectField("GetIconForGameObject", PrefabUtility.GetIconForGameObject(go), typeof(Texture), allowSceneObjects: true);
                EditorGUILayout.ObjectField("GetNearestPrefabInstanceRoot", PrefabUtility.GetNearestPrefabInstanceRoot(go), typeof(GameObject), allowSceneObjects: true);
                EditorGUILayout.ObjectField("GetOutermostPrefabInstanceRoot", PrefabUtility.GetOutermostPrefabInstanceRoot(go), typeof(GameObject), allowSceneObjects: true);
                EditorGUILayout.TextField("GetPrefabAssetPathOfNearestInstanceRoot", PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go));
                EditorGUILayout.LabelField("GetPrefabAssetType", PrefabUtility.GetPrefabAssetType(go).ToString());
                EditorGUILayout.ObjectField("GetPrefabInstanceHandle", PrefabUtility.GetPrefabInstanceHandle(go), typeof(GameObject), allowSceneObjects: true);
                EditorGUILayout.LabelField("GetPrefabInstanceStatus", PrefabUtility.GetPrefabInstanceStatus(go).ToString());

            }
            EditorGUILayout.EndScrollView();
        }
    }
}