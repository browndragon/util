using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;

namespace BDUtil.Serialization
{
    public static class ReflectionUtils
    {
        public static readonly Type ueb = typeof(UnityEventBase);
        // public List<BaseInvokableCall /* also private... */ > m_PersistentCalls.PrepareInvoke()
        public static readonly MethodInfo icl = ueb.GetMethod("PrepareInvoke", BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        static readonly Dictionary<Type, FieldInfo> backings = new();
        public static IEnumerable<Delegate> GetSubscribers(UnityEventBase @event)
        {
            object subs = icl.Invoke(@event, Array.Empty<object>());
            switch (subs)
            {
                case null: yield break;
                case IEnumerable e:
                    foreach (object o in e)
                    {
                        Type oType = o.GetType();
                        if (!backings.TryGetValue(oType, out FieldInfo fieldInfo))
                        {
                            foreach (EventInfo eventInfo in oType.GetEvents(BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy) ?? Array.Empty<EventInfo>())
                            {
                                fieldInfo = oType.GetField(eventInfo.Name, BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                                if (fieldInfo != null) { backings[oType] = fieldInfo; break; }
                            }
                        }
                        yield return (Delegate)fieldInfo.GetValue(o);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
