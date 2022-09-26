using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(InvokeAttribute))]
    public class InvokeAttributeDrawer : PropertyDrawer
    {
        static readonly Type buttonType = typeof(Invoke.Button);
        public new InvokeAttribute attribute => (InvokeAttribute)base.attribute;
        public bool IsButton => buttonType.IsAssignableFrom(fieldInfo.FieldType);
        public bool IsSuppressed
        {
            get
            {
                Invoke.Suppress current = default;
                if (EditorApplication.isPlaying) current |= Invoke.Suppress.Play;
                else current |= Invoke.Suppress.Editor;
                return attribute.Suppresses.HasFlag(current);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => IsButton
        ? IsSuppressed ? 0f : EditorGUIUtility.singleLineHeight
        : base.GetPropertyHeight(property, label);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.Log($"Drawing Invoke(isButton={IsButton},isSuppressed={IsSuppressed})@fieldType={fieldInfo.FieldType}");
            bool needs = false;
            if (IsButton)
            {
                if (!IsSuppressed) needs = GUI.Button(position, label);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property);
                needs = EditorGUI.EndChangeCheck();
            }

            object dirOwner = property.GetTargetObjectWithProperty();
            dirOwner.OrThrow();
            Type dirType = dirOwner.GetType();
            property.serializedObject.ApplyModifiedProperties();

            foreach (MethodInfo method in dirType
                .GetMethods(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(m => m.Name == attribute.MethodName)
            )
            {
                if (method == null) continue;
                if (method.GetParameters().Length != 0) continue;
                method.Invoke(dirOwner, Array.Empty<object>());
                break;
            }
        }
    }
}