using BDUtil.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Math.Editor
{
    [CustomPropertyDrawer(typeof(Randoms.Fuzzy<>))]
    public class RandomsFuzzyDrawer : InspectorUtils.WideDrawer
    {
        bool IsSupported(SerializedProperty property) => property.propertyType switch
        {
            SerializedPropertyType.Float => true,
            SerializedPropertyType.Integer => true,
            _ => false,
        };
        protected override bool WantsWide(
            SerializedProperty property,
            GUIContent label
        ) => base.WantsWide(property, label) && IsSupported(property.FindPropertyRelative("Pivot"));

        protected override float GetWideHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight;
        protected override float GetNarrowHeight(SerializedProperty property, GUIContent label)
        {
            var pivot = property.FindPropertyRelative("Pivot");
            var fuzz = property.FindPropertyRelative("Fuzz");
            if (IsSupported(pivot)) return InspectorUtils.GetLineHeight(2);
            return EditorGUIUtility.singleLineHeight
                + (property.isExpanded
                ? EditorGUIUtility.standardVerticalSpacing
                + EditorGUI.GetPropertyHeight(pivot, true)
                + EditorGUIUtility.standardVerticalSpacing
                + EditorGUI.GetPropertyHeight(fuzz, true)
                : 0f
            );
        }

        protected override void OnWideGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pivot = property.FindPropertyRelative("Pivot");
            var fuzz = property.FindPropertyRelative("Fuzz");
            OnSupportedGUI(position, pivot, fuzz, label);
        }
        protected override void OnNarrowGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pivot = property.FindPropertyRelative("Pivot");
            var fuzz = property.FindPropertyRelative("Fuzz");
            if (IsSupported(pivot))
            {
                OnSupportedGUI(position, pivot, fuzz, label);
                return;
            }
            Rect cursor = position;
            cursor.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(cursor, property.isExpanded, label);
            if (!property.isExpanded) return;
            cursor = InspectorUtils.NextVertical(cursor);
            label = EditorGUIUtility.wideMode ? pivotGC : shortPivotGC;
            cursor.height = EditorGUI.GetPropertyHeight(pivot, label);
            EditorGUI.PropertyField(cursor, pivot, label);
            cursor = InspectorUtils.NextVertical(cursor);
            label = EditorGUIUtility.wideMode ? fuzzGC : shortFuzzGC;
            cursor.height = EditorGUI.GetPropertyHeight(fuzz, label);
            EditorGUI.PropertyField(cursor, fuzz, label);
        }
        void OnSupportedGUI(Rect position, SerializedProperty pivot, SerializedProperty fuzz, GUIContent label)
        {
            scratchGC[0] = EditorGUIUtility.wideMode ? pivotGC : shortPivotGC;
            scratchGC[1] = EditorGUIUtility.wideMode ? fuzzGC : shortFuzzGC;
            switch (pivot.propertyType)
            {
                case SerializedPropertyType.Float:
                    scratchFloat[0] = pivot.floatValue;
                    scratchFloat[1] = fuzz.floatValue;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.MultiFloatField(position, label, scratchGC, scratchFloat);
                    if (EditorGUI.EndChangeCheck())
                    {
                        pivot.floatValue = scratchFloat[0];
                        fuzz.floatValue = scratchFloat[1];
                    }
                    return;
                case SerializedPropertyType.Integer:
                    scratchInt[0] = pivot.intValue;
                    scratchInt[1] = fuzz.intValue;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.MultiIntField(position, scratchGC, scratchInt);
                    if (EditorGUI.EndChangeCheck())
                    {
                        pivot.intValue = scratchInt[0];
                        fuzz.intValue = scratchInt[1];
                    }
                    return;
                default: throw pivot.propertyType.BadValue();
            }
        }


        static readonly GUIContent pivotGC = new("Pivot", "Central value");
        static readonly GUIContent fuzzGC = new("Fuzz", "Radius of cube of fuzz to apply");
        static readonly GUIContent shortPivotGC = new("P", "Pivot : Central value");
        static readonly GUIContent shortFuzzGC = new("F", "Fuzz : Radius of cube of fuzz to apply");
        static readonly GUIContent[] scratchGC = new GUIContent[2] { pivotGC, fuzzGC };
        static readonly float[] scratchFloat = new float[2];
        static readonly int[] scratchInt = new int[2];
    }
}