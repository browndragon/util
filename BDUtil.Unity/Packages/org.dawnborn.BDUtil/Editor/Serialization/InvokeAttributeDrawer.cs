using System;
using System.Collections.Generic;
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
            if (IsButton)
            {
                if (IsSuppressed) return;
                if (!GUI.Button(position, label)) return;
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, property);
                if (!EditorGUI.EndChangeCheck()) return;
            }

            InvokeNamedMethod(property, attribute.MethodName);
        }
        public static object InvokeNamedMethod(SerializedProperty property, string name, Type requireReturnType = null)
        {
            /// These are _barely_ used: the field name is duplicated for the button name.
            string fieldNamed; int index;
            object dirOwner = property.GetTargetParent(out fieldNamed, out index);
            dirOwner.OrThrow();

            Type dirType = dirOwner.GetType();
            property.serializedObject.ApplyModifiedProperties();
            bool NameMatches(MethodInfo m) => m.Name == name;
            List<MethodInfo> losers = new();
            foreach (MethodInfo method in dirType
                .GetMethods(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(NameMatches)
            )
            {
                if (method == null) continue;
                if (requireReturnType != null && !requireReturnType.IsAssignableFrom(method.ReturnType))
                {
                    losers.Add(method);
                    continue;
                }
                if (method.GetParameters().Length != 0)
                {
                    losers.Add(method);
                    continue;
                }
                return method.Invoke(method.IsStatic ? null : dirOwner, Array.Empty<object>());
            }
            Debug.LogWarning($"Couldn't find a method for `{(requireReturnType != null ? requireReturnType.ToString() : "")}{dirOwner}.{name}()` (try one of[{losers.Summarize()}] with wrong params/return type?)");
            return null;
        }
    }
}
