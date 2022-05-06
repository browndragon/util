using System.Collections.Generic;

namespace BDUtil
{
    /// Extension convenience methods.
    public static class Collections
    {
        public static void Add<K, V>(this ICollection<KeyValuePair<K, V>> thiz, K key, V value)
        => thiz.Add(new(key, value));
        public static void Add<R, C, V>(this ICollection<KeyValuePair<(R, C), V>> thiz, R row, C col, V value)
        => thiz.Add(new((row, col), value));

        public static bool Remove<K, V>(this ICollection<KeyValuePair<K, V>> thiz, K key, V value) => thiz.Remove(new(key, value));
        public static bool Remove<R, C, V>(this ICollection<KeyValuePair<(R, C), V>> thiz, R row, C col, V value) => thiz.Remove(new((row, col), value));

        public static void Swap<T, K, V>(this T thiz, K k1, K k2)
        where T : IDictionary<K, V>
        {
            switch (thiz.TryGetValue(k1, out var v1), thiz.TryGetValue(k2, out var v2))
            {
                case (true, true):
                    thiz[k1] = v2;
                    thiz[k2] = v1;
                    return;
                case (true, false):
                    thiz.Remove(k1);
                    thiz.Add(k2, v1);
                    return;
                case (false, true):
                    thiz.Add(k1, v2);
                    thiz.Remove(k2);
                    return;
                case (false, false): // Fallthrough
                default:
                    break;
            }
        }

        public static bool RemoveKey<K, V>(this IRemoveKey<K, V> thiz, K key) => thiz.RemoveKey(key, out var _);
        public static bool ContainsKey<K, V>(this ITryGetValue<K, V> thiz, K key) => thiz.TryGetValue(key, out var _);

        public static V GetValueOrDefault<K, V>(this ITryGetValue<K, V> thiz, K key, V @default = default) => thiz.TryGetValue(key, out var value) ? value : @default;
        public static V GetValueOrDefault<K, V>(this IReadOnlyDictionary<K, V> thiz, K key, V @default = default) => thiz.TryGetValue(key, out var value) ? value : @default;
        public static V GetValueOrDefault<T, K, V>(this T thiz, K key, V @default = default)
        where T : ITryGetValue<K, V>, IReadOnlyDictionary<K, V>
        => ((ITryGetValue<K, V>)thiz).TryGetValue(key, out var value) ? value : @default;

        public static V GetValueOrInsertNew<T, K, V>(this T thiz, K key)
        where T : ICollection<KeyValuePair<K, V>>, ITryGetValue<K, V>
        where V : new()
        {
            if (!thiz.TryGetValue(key, out var value))
            {
                value = new();
                thiz.Add(new(key, value));
            }
            return value;
        }

        // Multimap operations.
        public static void AddValue<T, K, TV, V>(this T thiz, K key, V value)
        where T : ICollection<KeyValuePair<K, TV>>, ITryGetValue<K, TV>
        where TV : ICollection<V>, new()
        => thiz.GetValueOrInsertNew<T, K, TV>(key).Add(value);

        public static bool RemoveValue<T, K, TV, V>(this T thiz, K key, V value)
        where T : ICollection<KeyValuePair<K, TV>>, ITryGetValue<K, TV>
        where TV : ICollection<V>, new()
        => thiz.TryGetValue(key, out var tv) && tv.Remove(value);


        public static string Summarize<T>(this IReadOnlyCollection<T> thiz, int limit = 5, string separator = ",")
        => Enumerables.Summarize(thiz, limit, separator, thiz.Count > limit ? $"... ({thiz.Count} total)" : default);
    }
}