using BDUtil.Editor;
using BDUtil.Serialization.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Math.Editor
{
    [CustomPropertyDrawer(typeof(HSVA))]
    public class HSVADrawer : InspectorUtils.WideDrawer
    {
        protected override float GetWideHeight(SerializedProperty property, GUIContent label)
        {
            return InspectorUtils.GetLineHeight(2);  // 1 for the colorpicker + HSVA-oneline.
        }
        protected override float GetNarrowHeight(SerializedProperty property, GUIContent label)
        {
            return InspectorUtils.GetLineHeight(5, 1);  // 1 for the colorpicker + HSVA.
        }
        protected override void OnWideGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label
        )
        {
            Rect cursor = position;
            cursor.height = EditorGUIUtility.singleLineHeight;
            HSVA has = (HSVA)property.GetTargetValue();
            EditorGUI.BeginChangeCheck();
            if (has.HasNaN()) EditorGUI.LabelField(cursor, label, noRGB);
            else has = EditorGUI.ColorField(cursor, label, has, true, true, false);
            if (EditorGUI.EndChangeCheck())
            {
                int i = 0;
                foreach (SerializedProperty ptr in property.Copy()) ptr.floatValue = has[i++];
            }
            cursor = InspectorUtils.NextVertical(cursor);

            EditorGUI.indentLevel++;

            Rect colorRect = EditorGUI.IndentedRect(cursor);
            colorRect.width /= 4;
            colorRect.width -= InspectorUtils.standardHorizontalSpacing;
            HSVA background = new(1f, 1f, 1f, 1f);
            for (int i = 0; i < 4; ++i)
            {
                background[i] = has[i];
                EditorGUI.DrawRect(colorRect, background);
                colorRect = InspectorUtils.NextHorizontal(colorRect);
            }

            EditorGUI.MultiPropertyField(cursor, SubLabels, property.FindPropertyRelative("h"), GUIContent.none);
            EditorGUI.indentLevel--;
        }
        protected override void OnNarrowGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label
        )
        {
            // Display a labeled color selector & then 4 float boxes with background color.
            Rect cursor = position;
            cursor.height = EditorGUIUtility.singleLineHeight;
            HSVA has = (HSVA)property.GetTargetValue();
            EditorGUI.BeginChangeCheck();
            if (has.HasNaN()) EditorGUI.LabelField(cursor, label, noRGB);
            else has = EditorGUI.ColorField(cursor, label, has, true, true, false);
            if (EditorGUI.EndChangeCheck())
            {
                int i = 0;
                foreach (SerializedProperty ptr in property.Copy()) ptr.floatValue = has[i++];
            }
            cursor = InspectorUtils.NextVertical(cursor);
            EditorGUI.indentLevel++;
            foreach (SerializedProperty ptr in property.Copy())
            {
                EditorGUI.PropertyField(cursor, ptr);
                cursor = InspectorUtils.NextVertical(cursor, margin: 0f);
            }
            EditorGUI.indentLevel--;
        }
        static readonly GUIContent noRGB = new("No color picker: HSV NaN values");
        static readonly GUIContent[] SubLabels = new GUIContent[4] {
            new("H", "Hue"),
            new("S", "Saturation"),
            new("V", "Value"),
            new("A", "Alpha")
        };
    }
}