using System;
using System.Collections.Generic;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// Any monobehaviour that implements IPickle can participate in filling out a pickle map, for later rehydration.
    public interface IPickle
    {
        /// If you return null, you won't depickle; index is the callth of your type.
        /// If you do want to e.g. cooperatively lazily store output in a T if singular or List<T> if plural,
        /// you'll need to handle that yourself (but see AggregateT and DisaggregateT).
        public object Pickle(object accum);
        /// The accum is the result of all previous pickle calls; the index the callth of your type.
        public void Unpickle(object accum, int index);
    }
    public interface IValuePickle<T> : IPickle
    where T : unmanaged
    {
        public T Value { get; set; }
        object IPickle.Pickle(object accum) => Value;
        void IPickle.Unpickle(object accum, int index) => Value = (T)accum;
    }
    public static class Pickles
    {
        static List<T> AddTo<T>(List<T> list, T t)
        { list.Add(t); return list; }
        /// Returns a T or a List<T>, flattening accumulated + element.
        public static object AggregateT<T>(object accumulated, T element) => accumulated switch
        {
            null => element,
            T other => new List<T> { other, element },
            List<T> other => AddTo(other, element),
            _ => throw new NotSupportedException(
                $"Unrecognized {accumulated}+{element}; first arg not null/T/List<T>..."
            ),
        };
        /// Returns a T from a null/T/List<T> by index.
        public static T DisaggregateT<T>(object accumulated, int index) => accumulated switch
        {
            null => default,
            T other => index == 0 ? other : default,
            List<T> other => index.IsInRange(0, other.Count) ? other[index] : default,
            _ => throw new NotSupportedException(
                $"Unrecognized {accumulated}[{index}]; first arg not null/T/List<T>..."
            )
        };
        public static void UnpickleFrom(this GameObject thiz, IEnumerable<KeyValuePair<Type, object>> pickles)
        {
            foreach (KeyValuePair<Type, object> param in pickles)
            {
                int i = 0;
                foreach (Component component in thiz.GetComponentsInChildren(param.Key))
                {
                    if (component is not IPickle pickled) throw new NotSupportedException($"{thiz}.{component} is of {param.Key} which isn't IPickle?!");
                    pickled.Unpickle(param.Value, i++);
                }
                if (i == 0) Debug.LogWarning($"{thiz} had pickle {param} but no matching components");
            }
        }
        public static void PickleInto(this GameObject thiz, IDictionary<Type, object> pickles)
        {

            foreach (IPickle pickled in thiz.GetComponentsInChildren<IPickle>())
            {
                Type type = pickled.GetType();
                if (!pickles.TryGetValue(type, out object pickle)) pickle = default;
                pickle = pickled.Pickle(pickle);
                if (pickle == null) pickles.Remove(type);
                else pickles[type] = pickle;
            }
        }
    }
}