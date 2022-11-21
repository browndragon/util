using BDUtil.Editor;
using BDUtil.Fluent;
using BDUtil.Serialization.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BDUtil.Play.Editor
{
    [CustomPropertyDrawer(typeof(CollisionMatrix))]
    public class CollisionMatrixDrawer : PropertyDrawer
    {
        static bool DefinedLayer(int i, out string name)
        {
            name = i < InternalEditorUtility.layers.Length ? InternalEditorUtility.layers[i] : null;
            return !name.IsEmpty();
        }
        static int NumLayers => InternalEditorUtility.layers.Length;

        static string[] DisplayLayers = null;
        static string[] CopyLayerNames()
        {
            if (DisplayLayers?.Length != InternalEditorUtility.layers?.Length) DisplayLayers = new string[InternalEditorUtility.layers.Length];
            for (int i = 0; i < InternalEditorUtility.layers?.Length; ++i) DisplayLayers[i] = InternalEditorUtility.layers[i];
            return DisplayLayers;
        }


        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        => property.isExpanded ? InspectorUtils.GetLineHeight(NumLayers + 1) : EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            CollisionMatrix layerMasks = (CollisionMatrix)property.GetTargetValue();
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if (!property.isExpanded) return;
            position = InspectorUtils.NextVertical(position);
            string[] layerNames = CopyLayerNames();
            int layerName = 0;
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < NumLayers; ++i)
            {
                if (!DefinedLayer(i, out string layername)) continue;
                int setMask = layerMasks.GetRHS(i);
                setMask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(setMask);
                setMask = EditorGUI.MaskField(
                    position, layername, setMask, layerNames
                );
                layerNames[layerName++] = null;
                setMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(setMask);
                layerMasks.SetRHS(i, setMask);
                position = InspectorUtils.NextVertical(position);
            }
            if (EditorGUI.EndChangeCheck()) property.SetTargetValue(layerMasks);
        }
    }
}