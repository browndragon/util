using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    public abstract class ChoiceDrawer<T> : PropertyDrawer
    {
        public struct Choices
        {
            public IReadOnlyList<T> Objects;
            public string[] Labels;
            public int Index;
            public void Deconstruct(out IReadOnlyList<T> objects, out string[] labels, out int index)
            {
                objects = Objects;
                labels = Labels;
                index = Index;
            }
        }
        protected abstract Choices GetChoices(SerializedProperty property);
        protected abstract void Update(SerializedProperty property, T update);
        protected virtual float InnerHeight(SerializedProperty property)
        => EditorGUIUtility.singleLineHeight;
        // Just label the field.
        // Alternatives (see ByRefDrawer) might instead recurse!
        protected virtual void DrawInnerField(Rect position, SerializedProperty property, GUIContent label)
        => EditorGUI.PrefixLabel(position, label);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Necessary per docs.
            EditorGUI.BeginProperty(position, label, property);
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
                (var objects, var labels, var prevIndex) = GetChoices(property);
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
                int currentIndex = EditorGUI.Popup(GetPopupPosition(position), prevIndex, labels);
                if (currentIndex < 0 || currentIndex >= objects.Count)
                {
                    Debug.LogWarning($"User selected {currentIndex} outside [0, {labels.Length}); suppressing");
                    currentIndex = prevIndex;
                }
                T selected = objects[currentIndex];
                Update(property, selected);
                DrawInnerField(position, property, label);
            }
            finally { EditorGUI.EndProperty(); }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects) return EditorGUIUtility.singleLineHeight;
            return InnerHeight(property);
        }
        Rect GetPopupPosition(Rect currentPosition)
        {
            Rect popupPosition = new(currentPosition);
            popupPosition.width -= EditorGUIUtility.labelWidth;
            popupPosition.x += EditorGUIUtility.labelWidth;
            popupPosition.height = EditorGUIUtility.singleLineHeight;
            return popupPosition;
        }
    }
}