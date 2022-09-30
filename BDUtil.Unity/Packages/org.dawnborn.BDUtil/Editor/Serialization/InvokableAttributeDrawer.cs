using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BDUtil.Math;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(Invokable.Layout))]
    public class InvokableAttributeDrawer : PropertyDrawer
    {
        static readonly Dictionary<(Type, string), List<(GUIContent, InvokableAttribute, Action<object>)>> cache = new();
        static readonly object[] noArgs = Array.Empty<object>();

        public List<(GUIContent, InvokableAttribute, Action<object>)> GetButtons(SerializedProperty property)
        {
            object parent = property.GetTargetParent(out string button, out int _);
            Type type = parent.GetType();
            if (cache.TryGetValue((type, button), out var buttons)) return buttons;
            buttons = new();
            HashSet<string> isNovel = new();
            for (Type ptype = type; ptype != null; ptype = ptype.BaseType)
            {
                foreach (MethodInfo method in ptype.GetMethods(BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    InvokableAttribute attribute = method.GetCustomAttribute<InvokableAttribute>();
                    TooltipAttribute tooltip = method.GetCustomAttribute<TooltipAttribute>();
                    if (attribute == null) continue;
                    if (!attribute.LayoutName.IsEmpty() && attribute.LayoutName != button) continue;
                    if (method.GetParameters().Length != 0) continue;
                    if (!isNovel.Add(method.Name)) continue;
                    buttons.Add((
                        new(attribute.Label ?? method.Name, tooltip?.tooltip),
                        attribute,
                        method.IsStatic
                        ? (o) => method.Invoke(null, noArgs)
                        : (o) => method.Invoke(o, noArgs)
                    ));
                }
            }
            buttons.Reverse();
            buttons.Sort((a, b) => Chain.Cmp || a.Item2.order.CompareTo(b.Item2.order) || a.Item1.text.CompareTo(b.Item1.text));
            return cache[(type, button)] = buttons;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => GetButtons(property).Count * EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent _)
        {
            Rect frame = position;
            frame.height = EditorGUIUtility.singleLineHeight;

            object dirOwner = property.GetTargetParent(out string buttonName, out int _buttonIndex);
            Invokable.Suppress current = default;
            if (EditorApplication.isPlaying) current |= Invokable.Suppress.Play;
            else current |= Invokable.Suppress.Editor;

            foreach ((GUIContent label, InvokableAttribute invokable, Action<object> invoke) in GetButtons(property))
            {
                bool suppressed = invokable.Suppresses.HasFlag(current);
                bool wasEnabled = GUI.enabled;
                GUI.enabled = !suppressed;
                if (GUI.Button(frame, label)) invoke(dirOwner);
                GUI.enabled = wasEnabled;
                frame.y += EditorGUIUtility.singleLineHeight;
            }
        }
        public static object InvokeNamedMethod(SerializedProperty property, string name, Type requireReturnType = null)
        {
            /// These are _barely_ used: the field name is duplicated for the button name.
            string fieldNamed; int index;
            object dirOwner = property.GetTargetParent(out fieldNamed, out index);
            dirOwner.OrThrow();

            Type dirType = dirOwner.GetType();
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
