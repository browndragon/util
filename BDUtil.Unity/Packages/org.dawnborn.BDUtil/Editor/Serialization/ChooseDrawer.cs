using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BDUtil.Serialization;
using BDUtil.Serialization.Editor;
using UnityEditor;
using UnityEngine;

namespace BDUtil.Editor
{
    /// Selects things which are tagged with Choice, allowing a method to populate the choices.
    [CustomPropertyDrawer(typeof(ChoiceAttribute))]
    public class ChooseDrawer : ChoiceDrawer
    {
        public new ChoiceAttribute attribute => (ChoiceAttribute)base.attribute;
        protected override Choices GetChoices(SerializedProperty property)
        {
            object retval = InvokableAttributeDrawer.InvokeNamedMethod(property, attribute?.MethodName, typeof(IEnumerable));
            if (retval is not IEnumerable @enum) throw new NotSupportedException($"Couldn't find {attribute?.MethodName} in {property}");
            int i = -1;
            List<GUIContent> labels = new();
            List<object> values = new();
            int hadChosen = -1;
            object hadValue = property.GetTargetValue();
            foreach (object o in @enum) HandleValue(i += 1, o, labels, values, hadValue, ref hadChosen);
            return new()
            {
                Objects = values,
                Labels = labels.ToArray(),
                Index = hadChosen,
            };
        }
        void HandleValue(int index, object value, List<GUIContent> labels, List<object> values, object hadValue, ref int hadChosen)
        {
            GUIContent display = null;
            object set = null;
            switch (value)
            {
                case null:
                    display = new("<null>");
                    set = null;
                    break;
                case string s: display = new((string)(set = s)); break;
                case ITuple t:
                    if (t.Length != 2) goto default;
                    display = t[0] switch
                    {
                        null => new("<null>"),
                        GUIContent c => c,
                        string s => new(s),
                        var x => new(x.ToString()),
                    };
                    set = t[1];
                    break;
                case IEnumerable eable:
                    IEnumerator etor = eable.GetEnumerator();
                    using (etor as IDisposable)
                    {
                        if (!etor.MoveNext()) goto default;
                        display = etor.Current switch
                        {
                            null => new("<null>"),
                            GUIContent c => c,
                            string s => new(s),
                            var x => new(x.ToString()),
                        };
                        if (!etor.MoveNext()) goto default;
                        set = etor.Current;
                    }
                    break;
                default: display = new((set = value)?.ToString()); break;
            }
            if (hadChosen < 0)
            {
                if (hadValue == null && set == null) hadValue = index;
                else if (hadValue?.Equals(set) ?? false) hadChosen = index;
            }
            labels.Add(display);
            values.Add(set);
        }

        protected override void Update(SerializedProperty property, Choices choices, int prev, int next)
        => property.SetTargetValue(choices.Objects[next]);
    }
}