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
            SerializedProperty posProp = property.FindPropertyRelative("position");
            SerializedProperty sizeProp = property.FindPropertyRelative("size");
            scratchGC[0] = new("Pos");
            scratchGC[1] = new("Size");
            scratchData[0] = posProp.floatValue;
            scratchData[1] = sizeProp.floatValue;
            EditorGUI.MultiFloatField(position, label, scratchGC, scratchData);
            posProp.floatValue = scratchData[0];
            sizeProp.floatValue = scratchData[1];
        }
        static readonly GUIContent[] scratchGC = new GUIContent[2];
        static readonly float[] scratchData = new float[2];
    }
}