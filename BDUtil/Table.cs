using System.Collections.Generic;

namespace BDUtil
{
    /// A 1:1 Dictionary, where the Key is decomposable into Row and Column parts.
    public class Table<R, C, V> : Map<(R r, C c), V>
    {
        readonly MultiMap<R, C> ColsByRows = new();
        readonly MultiMap<C, R> RowsByCols = new();

        public IReadOnlyMultiMap<R, C> Rows => ColsByRows;
        public IReadOnlyMultiMap<C, R> Cols => RowsByCols;

        public int DeleteRow(R r)
        {
            if (!ColsByRows.RemoveKey(r, out var other)) return 0;
            int deleted = 0;
            foreach (C c in other) deleted += Remove((r, c)) ? 1 : 0;
            return deleted;
        }
        public int DeleteCol(C c)
        {
            if (!RowsByCols.RemoveKey(c, out var other)) return 0;
            int deleted = 0;
            foreach (R r in other) deleted += Remove((r, c)) ? 1 : 0;
            return deleted;
        }

        protected override void RemoveEntry(Entry entry)
        {
            base.RemoveEntry(entry);
            if (!entry.HasValue) return;
            (R r, C c) = entry.Key;
            ColsByRows.Remove(new(r, c));
            RowsByCols.Remove(new(c, r));
        }
        protected override Entry TryAddEntry(KeyValuePair<(R r, C c), V> item)
        {
            var entry = base.TryAddEntry(item);
            if (!entry.HasValue) return entry;
            (R r, C c) = item.Key;
            ColsByRows.Add(new(r, c));
            RowsByCols.Add(new(c, r));
            return entry;
        }
    }
}