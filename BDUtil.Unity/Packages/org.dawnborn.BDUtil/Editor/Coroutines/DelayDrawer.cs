using BDUtil.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Coroutines.Editor
{
    [CustomPropertyDrawer(typeof(Delay))]
    public class DelayDrawer : InspectorUtils.WideDrawer
    {
        protected override float GetWideHeight(SerializedProperty property, GUIContent label)
        {
            return InspectorUtils.GetLineHeight(1) + 1;
        }
        protected override float GetNarrowHeight(SerializedProperty property, GUIContent label)
        {
            return InspectorUtils.GetLineHeight(2);
        }
        protected override void OnWideGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label
        )
        {
            Rect colorRect = position;
            colorRect.height = 1f;
            position.y += 1f;
            position.height -= 1f;
            position = EditorGUI.PrefixLabel(position, label);
            using var raw = InspectorUtils.Raw();
            LayoutOneLineHorizontally(raw, position, property, out float tick);
            DrawColorRect(colorRect, tick);
        }

        protected override void OnNarrowGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label
        )
        {
            position.height = EditorGUIUtility.singleLineHeight;
            Rect colorRect = EditorGUI.PrefixLabel(position, label);
            position = InspectorUtils.NextVertical(position);
            using var raw = InspectorUtils.Raw(indentLevel: -1);
            raw.IndentLevel++;
            LayoutOneLineHorizontally(raw, position, property, out float tick);
            DrawColorRect(colorRect, tick);
        }

        void LayoutOneLineHorizontally(InspectorUtils.RawScope raw, Rect position, SerializedProperty property, out float tick)
        {
            position.width -= 2 * InspectorUtils.standardHorizontalSpacing;
            position.width /= 3;

            SerializedProperty now = property.FindPropertyRelative("Now");
            Clock clock;
            now.enumValueIndex = (int)Enums<Clock>.GetValue(clock = (Clock)EditorGUI.EnumPopup(position, Enums<Clock>.FromValue(now.enumValueIndex)));
            position = InspectorUtils.NextHorizontal(position);

            SerializedProperty length = property.FindPropertyRelative("Length");
            raw.LabelWidth = InspectorUtils.GetLabelWidth(Length);
            length.floatValue = EditorGUI.FloatField(position, Length, length.floatValue);
            position = InspectorUtils.NextHorizontal(position);

            SerializedProperty start = property.FindPropertyRelative("Start");
            raw.LabelWidth = InspectorUtils.GetLabelWidth(Start);
            start.floatValue = EditorGUI.FloatField(position, Start, start.floatValue);

            if (!float.IsFinite(start.floatValue)) tick = start.floatValue;
            else if (float.IsNaN(length.floatValue)) tick = float.NaN;
            else if (float.IsInfinity(length.floatValue)) tick = 0f;
            else if (length.floatValue == 0f) tick = 1f;
            else
            {
                float timeNow = clock.GetTime();
                tick = (timeNow - start.floatValue) / length.floatValue;
            }
        }

        void DrawColorRect(Rect rect, float ratio)
        {
            Rect left = rect, right = left;
            Color lcolor = Color.grey, rcolor = Color.grey;
            if (ratio == float.NaN)
            {
                right.width = 0f;
            }
            else if (ratio < 0f)
            {
                rcolor = Color.red;
                if (ratio < -1f)
                {
                    left.width *= .1f;
                }
                else
                {
                    left.width = .1f + .8f * (1 + ratio);
                }
                right.width -= left.width;
                right.x = left.xMax;
            }
            else
            {
                lcolor = Color.green;
                if (ratio > 1f)
                {
                    left.width *= .9f;
                }
                else
                {
                    left.width = .1f + .8f * ratio;
                }
                right.width -= left.width;
                right.x = left.xMax;
            }
            EditorGUI.DrawRect(right, rcolor);
            EditorGUI.DrawRect(left, lcolor);
        }
        static readonly GUIContent Length = new("Len", "Duration in seconds");
        static readonly GUIContent Start = new("@", "Start time in seconds");
    }
}