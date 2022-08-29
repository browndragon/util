// using System.Diagnostics.CodeAnalysis;
// using UnityEditor;
// using UnityEngine;

// namespace BDUtil.Editor
// {
//     public static class PoolsEditor
//     {
//         [MenuItem("GameObject/UTI/Pool", false, 10), SuppressMessage("IDE", "IDE0051")]
//         static void CreatePool(MenuCommand menuCommand)
//         {
//             // Create a custom game object
//             GameObject go = new("Pool");
//             go.AddComponent<Pool>();
//             // Ensure it gets reparented if this was a context click (otherwise does nothing)
//             GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
//             // Register the creation in the undo system
//             Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
//             Selection.activeObject = go;
//         }
//         [MenuItem("GameObject/UTI/Registry", false, 10), SuppressMessage("IDE", "IDE0051")]
//         static void CreateRegistry(MenuCommand menuCommand)
//         {
//             // Create a custom game object
//             GameObject go = new("Registry");
//             go.AddComponent<Registry>();
//             // Ensure it gets reparented if this was a context click (otherwise does nothing)
//             GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
//             // Register the creation in the undo system
//             Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
//             Selection.activeObject = go;
//         }

//     }
// }