using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    // Psyche! It would make sense if this implemented ChoiceAttribute, but that's ChooseDrawer : ChoiceDrawer.
    // This also covers selecting type-restricted SubtypeDrawer.
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        => EditorGUI.GetPropertyHeight(property, label, true);

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}