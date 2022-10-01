using BDUtil.Fluent;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    // Psyche! It would make sense if this implemented ChoiceAttribute, but that's ChooseDrawer : ChoiceDrawer.
    // This also covers selecting type-restricted SubtypeDrawer.
    public abstract class ChoiceDrawer : PropertyDrawer
    {
        protected abstract Choices GetChoices(SerializedProperty property);
        protected abstract void Update(SerializedProperty property, Choices choices, int prevIndex, int index);
        protected virtual float InnerHeight(SerializedProperty property)
        => EditorGUI.GetPropertyHeight(property, true);

        protected virtual Rect DrawPrefixLabel(Rect position, SerializedProperty property, GUIContent label)
        => EditorGUI.PrefixLabel(position, label);
        // Just label the field.
        // Alternatives (see ByRefDrawer) might instead recurse!
        protected virtual void DrawInnerField(Rect position, SerializedProperty property, GUIContent label)
        { }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Necessary per docs.
            label = EditorGUI.BeginProperty(position, label, property);
            try
            {
                // Don't support multi!
                // If you try, the multiedit sets fields wrong after doing some amount of editing.
                // (I guess it uses wrong offset from wrong element?)
                if (property.serializedObject.isEditingMultipleObjects)
                {
                    EditorGUI.HelpBox(position, $"TODO: Can't multiedit choices", MessageType.Warning);
                    return;
                }
                Choices choices = GetChoices(property);
                (var objects, var labels, var prevIndex) = choices;
                if (objects == null || objects.Count <= 0 || labels == null || labels.Length <= 0)
                {
                    EditorGUI.HelpBox(position, $"Can't find choices for {property.propertyType}", MessageType.Warning);
                    return;
                }
                if (objects.Count != labels.Length)
                {
                    Debug.LogWarning($"{objects.Summarize()} != {labels.Summarize()} {objects.Count} vs {labels.Length}");
                    prevIndex = 0;
                }
                if (prevIndex < 0 || prevIndex >= objects.Count)
                {
                    Debug.LogWarning($"Can't find index {prevIndex} anymore!");
                    prevIndex = 0;
                }
                Rect remainder = DrawPrefixLabel(position, property, label);
                int currentIndex = EditorGUI.Popup(remainder, prevIndex, labels);
                if (currentIndex < 0 || currentIndex >= objects.Count)
                {
                    Debug.LogWarning($"User selected {currentIndex} outside [0, {labels.Length}); suppressing");
                    currentIndex = prevIndex;
                }
                Update(property, choices, prevIndex, currentIndex);
                DrawInnerField(position, property, label);
            }
            finally { EditorGUI.EndProperty(); }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return EditorGUIUtility.singleLineHeight;
            return InnerHeight(property);
        }
    }
}