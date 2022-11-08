using UnityEditor;
using UnityEngine;

namespace BDUtil.Math.Editor
{
    [CustomPropertyDrawer(typeof(Extent))]
    public class ExtentDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");
            scratchGC[0] = new("Min");
            scratchGC[1] = new("Max");
            scratchData[0] = minProp.floatValue;
            scratchData[1] = maxProp.floatValue;
            EditorGUI.MultiFloatField(position, label, scratchGC, scratchData);
            minProp.floatValue = scratchData[0];
            maxProp.floatValue = scratchData[1];
        }
        static readonly GUIContent[] scratchGC = new GUIContent[2];
        static readonly float[] scratchData = new float[2];
    }
}