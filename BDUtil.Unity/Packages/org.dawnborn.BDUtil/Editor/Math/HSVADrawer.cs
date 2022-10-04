using System;
using System.Collections;
using BDUtil.Fluent;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Math.Editor
{
    [CustomPropertyDrawer(typeof(HSVA))]
    public class HSVADrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property,
                                                GUIContent label)
        => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position,
                                   SerializedProperty property,
                                   GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            HSVA color = default;
            int i = 0;
            SerializedProperty pristine = property;
            property = property.Copy();
            foreach (SerializedProperty component in property)
            {
                if (component.name != Labels[i].text.ToLower()) throw new ArgumentException($"{component.name} isn't {Labels[i].text}!");
                color[i] = Values[i] = component.floatValue;
                i += 1;
            }
            (i == 4).OrThrow();

            Rect colorRect = position;
            colorRect.x += EditorGUIUtility.labelWidth;
            colorRect.width -= EditorGUIUtility.labelWidth;
            colorRect.width /= 4f;
            float margin = 4f;
            colorRect.width -= margin;
            for (i = 0; i < 3; ++i)
            {
                HSVA isolate = new(1f, 0f, 0f, 1f);
                for (int j = 2; j >= i; --j) isolate[j] = color[j];
                EditorGUI.DrawRect(colorRect, isolate);
                colorRect.x += colorRect.width + margin;
            }
            EditorGUI.DrawRect(colorRect, color);


            EditorGUI.BeginChangeCheck();
            EditorGUI.MultiFloatField(position, label, Labels, Values);
            if (EditorGUI.EndChangeCheck())
            {
                i = 0;
                foreach (SerializedProperty component in pristine)
                {
                    Debug.Log($"Updating {component.name} {component.floatValue}=>{Values[i]}");
                    component.floatValue = Values[i];
                    i += 1;
                }
            }

            EditorGUI.EndProperty();
        }
        static readonly GUIContent[] Labels = new GUIContent[] {
            new("H", "Hue"),
            new("S", "Saturation"),
            new("V", "Value/Brightness"),
            new("A", "Alpha/Transparency")
        };
        static readonly float[] Values = new float[4];
    }
}