using System;
using BDUtil.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Math.Editor
{
    [CustomPropertyDrawer(typeof(Interval))]
    public class IntervalDrawer : PropertyDrawer
    {
        public enum DisplayModes
        {
            MinMax = default,
            CenterRadius,
        }
        public static DisplayModes DisplayMode;
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            SerializedProperty minProp = property.FindPropertyRelative("min");
            SerializedProperty maxProp = property.FindPropertyRelative("max");
            scratchData[0] = minProp.floatValue;
            scratchData[1] = maxProp.floatValue;
            position = EditorGUI.PrefixLabel(position, label);
            using (InspectorUtils.Raw())
            {
                Rect popup = position;
                popup.width = 32f;
                DisplayMode = (DisplayModes)EditorGUI.EnumPopup(popup, DisplayMode);
                popup.width += InspectorUtils.standardHorizontalSpacing;
                position.x = popup.xMax;
                position.width -= popup.width;
                switch (DisplayMode)
                {
                    case DisplayModes.MinMax: OnGUIMinMax(position); break;
                    case DisplayModes.CenterRadius: OnGUICR(position); break;
                    default: throw DisplayMode.BadValue();
                }
            }
            minProp.floatValue = scratchData[0];
            maxProp.floatValue = scratchData[1];
        }

        private void OnGUICR(Rect position)
        {
            (scratchData[0], scratchData[1]) = (
                (scratchData[1] + scratchData[0]) / 2f,
                (scratchData[1] - scratchData[0]) / 2f
            );
            EditorGUI.MultiFloatField(position, CenterRadiusLabels, scratchData);
            (scratchData[0], scratchData[1]) = (
                scratchData[0] - scratchData[1],
                scratchData[0] + scratchData[1]
            );
        }

        private void OnGUIMinMax(Rect position)
        => EditorGUI.MultiFloatField(position, MinMaxLabels, scratchData);

        static readonly GUIContent[] MinMaxLabels = new GUIContent[] { new("Min", "Least value"), new("Max", "Most value") };
        static readonly GUIContent[] CenterRadiusLabels = new GUIContent[] { new("C", "Central value"), new("R", "Radius or Extent; half the size") };
        static readonly float[] scratchData = new float[2];
    }
}