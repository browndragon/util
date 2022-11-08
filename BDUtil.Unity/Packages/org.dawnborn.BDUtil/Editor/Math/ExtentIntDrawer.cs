using UnityEditor;
using UnityEngine;

namespace BDUtil.Math.Editor
{
    [CustomPropertyDrawer(typeof(ExtentInt))]
    public class ExtentIntDrawer : PropertyDrawer
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
            scratchData[0] = minProp.intValue;
            scratchData[1] = maxProp.intValue;
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.MultiIntField(position, scratchGC, scratchData);
            minProp.intValue = scratchData[0];
            maxProp.intValue = scratchData[1];
        }
        static readonly GUIContent[] scratchGC = new GUIContent[2];
        static readonly int[] scratchData = new int[2];
    }
}