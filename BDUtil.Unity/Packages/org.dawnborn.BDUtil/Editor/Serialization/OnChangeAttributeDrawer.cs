using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(OnChangeAttribute))]
    public class OnChangedCallAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool asButton = true;
            foreach (OnChangeAttribute at in fieldInfo.GetCustomAttributes<OnChangeAttribute>()) asButton &= at.AsButton;
            return asButton ? EditorGUIUtility.singleLineHeight : base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool asButton = true;
            foreach (OnChangeAttribute at in fieldInfo.GetCustomAttributes<OnChangeAttribute>()) asButton &= at.AsButton;
            bool needs = false;
            if (asButton)
            {
                needs = GUI.Button(position, label);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property);
                needs = EditorGUI.EndChangeCheck();
            }
            if (!needs) return;
            OnChangeAttribute.Suppress current = default;
            if (EditorApplication.isPlaying) current |= OnChangeAttribute.Suppress.Play;
            else current |= OnChangeAttribute.Suppress.Editor;

            object dirOwner = property.GetTargetObjectWithProperty();
            dirOwner.OrThrow();
            Type dirType = dirOwner.GetType();
            property.serializedObject.ApplyModifiedProperties();
            foreach (OnChangeAttribute at in fieldInfo.GetCustomAttributes<OnChangeAttribute>())
            {
                if ((at.Suppresses & current) != OnChangeAttribute.Suppress.Never)
                {
                    continue;
                }
                foreach (MethodInfo method in dirType
                    .GetMethods(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(m => m.Name == at.MethodName)
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
}