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
            SerializedProperty posProp = property.FindPropertyRelative("position");
            SerializedProperty sizeProp = property.FindPropertyRelative("size");
            scratchGC[0] = new("Pos");
            scratchGC[1] = new("Size");
            scratchData[0] = posProp.intValue;
            scratchData[1] = sizeProp.intValue;
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.MultiIntField(position, scratchGC, scratchData);
            posProp.intValue = scratchData[0];
            sizeProp.intValue = scratchData[1];
        }
        static readonly GUIContent[] scratchGC = new GUIContent[2];
        static readonly int[] scratchData = new int[2];
    }
}