using BDUtil.Fluent;
using UnityEditor;
using UnityEngine;
namespace BDUtil.Editor
{
    public static class InspectorUtils
    {
        public static float standardFieldWidth = 80f;
        public static float standardHorizontalSpacing = 1.5f * EditorGUIUtility.standardVerticalSpacing;
        public static bool HasWidthFor(float width) => UnityEngine.Screen.width > width;

        public static Rect NextVertical(in Rect prev, float height = float.NaN, float margin = float.NaN)
        => new(
            prev.x,
            prev.y + (float.IsFinite(height) ? height : prev.height) + (float.IsFinite(margin) ? margin : EditorGUIUtility.standardVerticalSpacing),
            prev.width,
            prev.height
        );
        public static Rect NextHorizontal(in Rect prev, float width = float.NaN, float margin = float.NaN)
        => new(
            prev.x + (float.IsFinite(width) ? width : prev.width) + (float.IsFinite(margin) ? margin : standardHorizontalSpacing),
            prev.y,
            prev.width,
            prev.height
        );
        public static float GetLineHeight(int numLines, int numSeparators = -1)
        {
            if (numSeparators < 0) numSeparators += numLines;
            return numLines * EditorGUIUtility.singleLineHeight
            + numSeparators * EditorGUIUtility.standardVerticalSpacing;
        }
        public static float GetLineWidth(int numElements, bool incLabel = true, int numSeparators = -1)
        {
            if (numSeparators < 0) numSeparators += numElements;
            return numElements * standardFieldWidth
                + numSeparators * standardHorizontalSpacing
                + (incLabel ? Mathf.Max(standardFieldWidth, EditorGUIUtility.labelWidth) : 0f);
        }
        public abstract class WideDrawer : PropertyDrawer
        {
            protected virtual bool WantsWide(
                SerializedProperty property,
                GUIContent label
            ) => EditorGUIUtility.wideMode;

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => WantsWide(property, label)
            ? GetWideHeight(property, label)
            : GetNarrowHeight(property, label);
            protected virtual float GetWideHeight(SerializedProperty property, GUIContent label)
            => GetLineHeight(1);
            protected virtual float GetNarrowHeight(SerializedProperty property, GUIContent label)
            => GetLineHeight(2);

            protected void OnBaseGUI(Rect position, SerializedProperty property, GUIContent label)
            => base.OnGUI(position, property, label);
            public override void OnGUI(Rect position,
                                       SerializedProperty property,
                                       GUIContent label)
            {
                if (WantsWide(property, label)) OnWideGUI(position, property, label);
                else OnNarrowGUI(position, property, label);
            }
            protected abstract void OnWideGUI(
                Rect position,
                SerializedProperty property,
                GUIContent label
            );
            protected abstract void OnNarrowGUI(
                Rect position,
                SerializedProperty property,
                GUIContent label
            );
        }
    }
}