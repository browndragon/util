using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    [CustomPropertyDrawer(typeof(Holder<>))]
    public class InlineDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Data"));
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Data"), label);
        }
    }
}