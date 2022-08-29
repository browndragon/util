using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    [CustomPropertyDrawer(typeof(Ref<>))]
    public class RefDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Data"));
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var requiredType = this.fieldInfo.FieldType.GetUnderlyingType();
            SerializedProperty unity = property.FindPropertyRelative("Data");
            Rect subrect = position;
            // Begin drawing property field.
            EditorGUI.BeginProperty(subrect, label, unity);
            // Draw property field with restricted type.
            unity.objectReferenceValue = EditorGUI.ObjectField(
                subrect, label, unity.objectReferenceValue, requiredType, property.serializedObject.targetObject
            );
            // Finish drawing property field.
            EditorGUI.EndProperty();
        }
    }
}