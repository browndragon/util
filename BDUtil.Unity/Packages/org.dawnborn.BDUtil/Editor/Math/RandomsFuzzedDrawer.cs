using BDUtil.Fluent;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Math.Editor
{
    [CustomPropertyDrawer(typeof(Randoms.Fuzzed<>))]
    public class RandomsFuzzedDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        => property.FindPropertyRelative("Pivot").propertyType switch
        {
            SerializedPropertyType.Float => EditorGUIUtility.singleLineHeight,
            SerializedPropertyType.Integer => EditorGUIUtility.singleLineHeight,
            _ => base.GetPropertyHeight(property, label),
        };

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            var pivot = property.FindPropertyRelative("Pivot");
            switch (pivot.propertyType)
            {
                case SerializedPropertyType.Float:  // fallthrough
                case SerializedPropertyType.Integer:
                    EditorGUI.MultiPropertyField(position, scratchGC, pivot, label);
                    break;
                default: base.OnGUI(position, property, label); break;
            }
        }
        static readonly GUIContent[] scratchGC = new GUIContent[2] { new("P", "Pivot/Center value"), new("S", "Scale/Max-Min value") };
    }
}