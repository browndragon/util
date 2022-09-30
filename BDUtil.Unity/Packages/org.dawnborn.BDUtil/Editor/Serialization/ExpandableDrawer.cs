// Per https://forum.unity.com/threads/editor-tool-better-scriptableobject-inspector-editing.484393/ .
using System;
using System.Collections.Generic;
using BDUtil.Serialization.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization
{
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer : PropertyDrawer
    {
        public enum BackgroundStyles
        {
            None,
            HelpBox,
            Darken,
            Lighten
        }

        public static readonly bool SHOW_SCRIPT_FIELD = false;

        /// <summary>
        /// The spacing on the inside of the background rect.
        /// </summary>
        public static readonly float INNER_SPACING = 6.0f;

        /// <summary>
        /// The spacing on the outside of the background rect.
        /// </summary>
        public static readonly float OUTER_SPACING = 4.0f;
        public static readonly float DROPDOWN_AFFORDANCE = 32f;

        /// <summary>
        /// The style the background uses.
        /// </summary>
        public static readonly BackgroundStyles BACKGROUND_STYLE = BackgroundStyles.HelpBox;

        /// <summary>
        /// The colour that is used to darken the background.
        /// </summary>
        public static readonly Color DARKEN_COLOUR = new(0.0f, 0.0f, 0.0f, 0.2f);

        /// <summary>
        /// The colour that is used to lighten the background.
        /// </summary>
        public static readonly Color LIGHTEN_COLOUR = new(1.0f, 1.0f, 1.0f, 0.2f);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = 0.0f;
            totalHeight += EditorGUIUtility.singleLineHeight;
            if (property.objectReferenceValue == null) return totalHeight;
            if (!property.isExpanded) return totalHeight;
            SerializedObject targetObject = new(property.objectReferenceValue);

            if (targetObject == null) return totalHeight;
            SerializedProperty field = targetObject.GetIterator();
            field.NextVisible(true);
            if (SHOW_SCRIPT_FIELD)
            {
                totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            while (field.NextVisible(false))
            {
                totalHeight += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            totalHeight += INNER_SPACING * 2;
            totalHeight += OUTER_SPACING * 2;

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Don't support multi!
            // If you try, the multiedit sets fields wrong after doing some amount of editing.
            // (I guess it uses wrong offset from wrong element?)
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(position, $"TODO: Can't multiedit choices", MessageType.Warning);
                return;
            }
            Rect fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;
            Rect dropdownRect = fieldRect;
            Type underlying = fieldInfo.FieldType.GetUnderlyingType();
            Type showDropdown = typeof(ScriptableObject);

            if (showDropdown.IsAssignableFrom(underlying)) showDropdown = underlying;
            else if (!underlying.IsAssignableFrom(showDropdown)) showDropdown = null;

            if (showDropdown != null)
            {
                dropdownRect.width = DROPDOWN_AFFORDANCE;
                dropdownRect.x = fieldRect.x + fieldRect.width - DROPDOWN_AFFORDANCE;
                fieldRect.width -= DROPDOWN_AFFORDANCE;
            }
            EditorGUI.PropertyField(fieldRect, property, label, true);
            if (showDropdown != null)
            {
                Choices choices = Choices.Get(new TypeKey(showDropdown,
                    opt0: new(" + ", "Use the object selector, or select a different option to create a new object"),
                    serializable: false, instantiable: true, unityTypes: true)
                );
                int selected = EditorGUI.Popup(dropdownRect, 0, choices.Labels);
                if (selected > 0)
                {
                    Type chosen = (Type)choices.Objects[selected];
                    ScriptableObject created = EditorUtils.CreateScriptableObjectOfType(chosen, true);
                    property.objectReferenceValue = created;
                }
            }

            if (property.objectReferenceValue == null)
                return;
            property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, GUIContent.none, true);

            if (!property.isExpanded)
                return;

            SerializedObject targetObject = new(property.objectReferenceValue);

            if (targetObject == null)
                return;


            #region Format Field Rects
            List<Rect> propertyRects = new();
            Rect marchingRect = fieldRect;

            Rect bodyRect = fieldRect;
            bodyRect.xMin += EditorGUI.indentLevel * 14;
            bodyRect.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
                + OUTER_SPACING;

            SerializedProperty field = targetObject.GetIterator();
            field.NextVisible(true);

            marchingRect.y += INNER_SPACING + OUTER_SPACING;

            if (SHOW_SCRIPT_FIELD)
            {
                propertyRects.Add(marchingRect);
                marchingRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            while (field.NextVisible(false))
            {
                marchingRect.y += marchingRect.height + EditorGUIUtility.standardVerticalSpacing;
                marchingRect.height = EditorGUI.GetPropertyHeight(field, true);
                propertyRects.Add(marchingRect);
            }

            marchingRect.y += INNER_SPACING;

            bodyRect.yMax = marchingRect.yMax;
            #endregion

            DrawBackground(bodyRect);

            #region Draw Fields
            EditorGUI.indentLevel++;

            int index = 0;
            field = targetObject.GetIterator();
            field.NextVisible(true);

            if (SHOW_SCRIPT_FIELD)
            {
                //Show the disabled script field
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(propertyRects[index], field, true);
                EditorGUI.EndDisabledGroup();
                index++;
            }

            //Replacement for "editor.OnInspectorGUI ();" so we have more control on how we draw the editor
            while (field.NextVisible(false))
            {
                try
                {
                    EditorGUI.PropertyField(propertyRects[index], field, true);
                }
                catch (StackOverflowException)
                {
                    field.objectReferenceValue = null;
                    Debug.LogError("Detected self-nesting cauisng a StackOverflowException, avoid using the same " +
                        "object iside a nested structure.");
                }

                index++;
            }

            targetObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
            #endregion
        }

        /// <summary>
        /// Draws the Background
        /// </summary>
        /// <param name="rect">The Rect where the background is drawn.</param>
        private void DrawBackground(Rect rect)
        {
            switch (BACKGROUND_STYLE)
            {

                case BackgroundStyles.HelpBox:
                    EditorGUI.HelpBox(rect, "", MessageType.None);
                    break;

                case BackgroundStyles.Darken:
                    EditorGUI.DrawRect(rect, DARKEN_COLOUR);
                    break;

                case BackgroundStyles.Lighten:
                    EditorGUI.DrawRect(rect, LIGHTEN_COLOUR);
                    break;
            }
        }
    }
}