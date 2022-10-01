using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BDUtil.Clone;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace BDUtil.Serialization.Editor
{
    public class PrefabDetector : EditorWindow
    {
        [MenuItem("Tools/PrefabDetector")]
        static void ShowToolbar()
        {
            EditorWindow.GetWindow<PrefabDetector>("PrefabDetector");
        }

        private Vector2 scroll;
        public void OnGUI()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                EditorGUILayout.HelpBox("Select a gameobject to examine.", MessageType.Info);
                return;
            }

            EditorGUIUtility.labelWidth = 300;
            scroll = EditorGUILayout.BeginScrollView(scroll);
            {
                EditorGUILayout.ObjectField("Selection", go, typeof(GameObject), allowSceneObjects: true);
                EditorGUILayout.LabelField("IDStr", go.IDStr());

                EditorGUILayout.Space();
                Cloned cloned = go.GetComponent<Cloned>();
                if (cloned != null)
                {
                    EditorGUILayout.LabelField("Cloned", EditorStyles.boldLabel);

                    EditorGUILayout.ObjectField("Root", cloned.Root, typeof(GameObject), allowSceneObjects: true);
                    EditorGUILayout.LabelField("Root.IDStr", cloned.Root?.IDStr() ?? "");
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