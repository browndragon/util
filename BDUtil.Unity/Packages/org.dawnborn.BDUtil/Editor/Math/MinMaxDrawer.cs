using System.Reflection;
using BDUtil.Serialization.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Math.Editor
{
    [
        CustomPropertyDrawer(typeof(MinMax)),
        CustomPropertyDrawer(typeof(MinMax.RangeAttribute))
    ]
    public class MinMaxDrawer : PropertyDrawer
    {
        static readonly GUIContent[] scratchGC = new GUIContent[2];
        static readonly float[] scratchFloat = new float[2];

        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            bool isVector2 = property.propertyType == SerializedPropertyType.Vector2;
            SerializedProperty
                minProp = isVector2 ? null : property.FindPropertyRelative("Min"),
                maxProp = isVector2 ? null : property.FindPropertyRelative("Max");
            float
                min = isVector2 ? property.vector2Value.x : minProp.floatValue,
                max = isVector2 ? property.vector2Value.y : maxProp.floatValue,
                minLimit = 0f,
                maxLimit = 1f;
            MinMax.RangeAttribute.Displays display = default;
            MinMax.RangeAttribute range = fieldInfo.GetCustomAttribute<MinMax.RangeAttribute>();
            if (range != null)
            {
                display = range.Display;
                minLimit = range.Min;
                maxLimit = range.Max;
            }
            if (!float.IsFinite(minLimit) || !float.IsFinite(maxLimit))
            {
                display = MinMax.RangeAttribute.Displays.Vector2;
            }
            if (display == MinMax.RangeAttribute.Displays.Vector2)
            {
                scratchGC[0] = new("Min", $"Min > {minLimit}");
                scratchGC[1] = new("Max", $"Max < {maxLimit}");
                scratchFloat[0] = min;
                scratchFloat[1] = max;
                EditorGUI.MultiFloatField(position, label, scratchGC, scratchFloat);
                min = Mathf.Max(minLimit, scratchFloat[0]);
                max = Mathf.Min(maxLimit, scratchFloat[1]);
            }
            else
            {
                if (display == MinMax.RangeAttribute.Displays.LogSlider)
                {
                    min = Mathf.Log10(min);
                    max = Mathf.Log10(max);
                    minLimit = Mathf.Log10(minLimit);
                    maxLimit = Mathf.Log10(maxLimit);
                }
                EditorGUI.MinMaxSlider(position, label, ref min, ref max, minLimit, maxLimit);
                if (display == MinMax.RangeAttribute.Displays.LogSlider)
                {
                    min = Mathf.Pow(10, min);
                    max = Mathf.Pow(10, max);
                }
            }

            if (isVector2)
            {
                property.vector2Value = new(min, max);
            }
            else
            {
                minProp.floatValue = min;
                maxProp.floatValue = max;
            }
        }
    }
}