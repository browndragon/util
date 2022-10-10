using BDUtil.Fluent;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    // Psyche! It would make sense if this implemented ChoiceAttribute, but that's ChooseDrawer : ChoiceDrawer.
    // This also covers selecting type-restricted SubtypeDrawer.
    [CustomPropertyDrawer(typeof(OrNil<>))]
    public class OrNilDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"), label);

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            SerializedProperty HasValue = property.FindPropertyRelative("HasValue").OrThrow();
            SerializedProperty Value = property.FindPropertyRelative("value").OrThrow();

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, Value, label);
            if (EditorGUI.EndChangeCheck()) HasValue.boolValue = true;

            EditorGUI.BeginChangeCheck();
            position.height = EditorGUIUtility.singleLineHeight;
            position.x -= position.height - 4f;
            HasValue.boolValue = EditorGUI.Toggle(position, none, HasValue.boolValue);
            // bool wasChanged =
            EditorGUI.EndChangeCheck();
            // if (wasChanged)
            // {
            //     // TODO: I wish we could easily trigger a redraw here
            // }
        }
        static readonly GUIContent none = new((string)null, "If enabled AND NULL, will clear values");
    }
}