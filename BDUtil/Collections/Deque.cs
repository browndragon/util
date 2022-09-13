using System;
using System.Collections.Generic;
using BDUtil.Math;

namespace BDUtil.Raw
{
    /// Code to treat ILists as stacks & queues.
    /// It's always safe to treat a List or Deque as a Stack; it's only efficient to treat a Deque as a Queue.
    public static class Deques
    {
        public static T PeekIndex<T>(this IList<T> thiz, int index, T @default = default)
        => index.IsInRange(0, thiz.Count) ? thiz[index] : @default;
        public static bool PopIndex<T>(this IList<T> thiz, int index, out T value)
        {
            if (!index.IsInRange(0, thiz.Count)) return false.Let(value = default);
            value = thiz[index];
            thiz.RemoveAt(index);
            return true;
        }
        public static T PopIndexOrDefault<T>(this IList<T> thiz, int index, T @default = default)
        => thiz.PopIndex(index, out T value) ? value : @default;

        public enum Ends
        {
            Back = default,  // AKA "stack"
            Front,  // AKA "queue"
        }

        /// At the first or last element; [0, Count).
        public static int GetReadIndex(this Ends thiz, int count)
        => thiz switch { Ends.Front => 0, _ => count - 1 };
        /// Before the first or after the last; [0, Count].
        public static int GetInsertIndex(this Ends thiz, int count)
        => thiz switch { Ends.Front => 0, _ => count };
        public static Ends GetOtherEnd(this Ends thiz)
        => thiz switch { Ends.Front => Ends.Back, _ => Ends.Front };

        public static T PeekAt<T>(this IList<T> thiz, Ends end, T @default = default)
        => thiz.PeekIndex(end.GetReadIndex(thiz.Count), @default);
        public static bool PopFrom<T>(this IList<T> thiz, Ends end, out T value)
        => thiz.PopIndex(end.GetReadIndex(thiz.Count), out value);
        public static T PopFromOrDefault<T>(this IList<T> thiz, Ends popFrom, T @default = default)
        => thiz.PopFrom(popFrom, out T t) ? t : @default;
        public static T PushTo<T>(this IList<T> thiz, Ends end, T value)
        {
            thiz.Insert(end.GetInsertIndex(thiz.Count), value);
            return value;
        }

        public static T PopFront<T>(this IList<T> thiz, T @default = default)
        => thiz.PopFromOrDefault(Ends.Front, @default);
        public static T PopBack<T>(this IList<T> thiz, T @default = default)
        => thiz.PopFromOrDefault(Ends.Back, @default);
        public static T PushFront<T>(this IList<T> thiz, T item)
        => thiz.PushTo(Ends.Front, item);
        public static T PushBack<T>(this IList<T> thiz, T item)
        => thiz.PushTo(Ends.Back, item);
        public static T PeekFront<T>(this IList<T> thiz, T @default = default)
        => thiz.PeekAt(Ends.Front, @default);
        public static T PeekBack<T>(this IList<T> thiz, T @default = default)
        => thiz.PeekAt(Ends.Back, @default);

        /// Destructive enumeration as stack. Safe for interleaving edits!
        public static IEnumerable<T> PopDeque<T>(this IList<T> thiz, Ends end)
        {
            while (thiz.PopFrom(end, out T t)) yield return t;
        }
        public static int Capacity<T>(this IReadOnlyList<T> thiz) => thiz switch
        {
            null => 0,
            Array a => a.Length,
            List<T> l => l.Capacity,
            Deque<T> d => d.Capacity,
            _ => throw new NotSupportedException($"Unrecognized {thiz.GetType()}"),
        };
        public static void TrimExcess<T>(this IList<T> thiz)
        {
            switch (thiz)
            {
                case null: return;
                case Array _: throw new NotSupportedException();
                case List<T> l: l.TrimExcess(); return;
                case Deque<T> d: d.TrimExcess(); return;
                default: throw new NotSupportedException($"Unrecognized {thiz.GetType()}");
            }
        }
        public static bool PushOrPop<T>(this IList<T> thiz, Ends end, T value, out T oldValue)
        {
            if (thiz.Count < ((IReadOnlyList<T>)thiz).Capacity())
            {
                thiz.PushTo(end, value);
                return false.Let(oldValue = default);
            }
            oldValue = thiz.PopFromOrDefault(end.GetOtherEnd());
            thiz.PushTo(end, value);
            return true;
        }
    }
    /// A list whose 0 indices can migrate forward & back.
    /// This doesn't need to implement the Pop/Push operations directly, which can be generically implemented for all List-y types.
    public class Deque<T> : AList<T>
    {
        // public so it's natively unity serializable. You really shouldn't adjust these though!
        T[] _Data;
        int _Head = 0;

        /// Total amount this can hold before needing to grow.
        public int Capacity => _Data?.Length ?? 0;
        // If limit is positive, EnsureCapacity can't grow past it.
        // Resize still can!
        public int Limit = 0;

        int GetOffset(int? offset = default) => (_Head + (offset ?? Count)).PosMod(Capacity);

        public Deque(int size) => Resize(size);
        public Deque() : this(16) { }
        public void Resize(int newSize)
        {
            if (_Data?.Length == newSize) return;
            if (newSize < 0) newSize = 0;
            T[] newData = null;
            if (newSize > 0)
            {
                newData = new T[newSize];
                newSize = newData.Length;
                if (Count < newSize) newSize = Count;
                for (int i = 0; i < newSize; ++i) newData[i] = this[i];
            }
            _Data = newData;
            _Head = 0;
            Count = newSize;
        }

        public override T this[int i]
        {
            get => _Data[GetOffset(i.CheckRange(0, Count))];
            set => _Data[GetOffset(i.CheckRange(0, Count))] = value;
        }

        public override void RemoveAt(int index)
        {
            index.CheckRange(0, Count);
            if (index >= Count / 2) for (
                int i = index + 1; i < Count; ++i
            ) this[i - 1] = this[i];
            else
            {
                for (int i = index - 1; i >= 0; --i) this[i + 1] = this[i];
                _Head = GetOffset(1);
            }
            Count -= 1;
        }

        // Insert so that `this[index]==item` afterwards.
        public override void Insert(int index, T item)
        {
            if (Count >= Capacity) EnsureCapacity();
            index.CheckRangeInclusive(0, Count);
            // Ok, now make room to insert the element we're going to add.
            // the element is actually pushed back now (but default).
            if (Count++ == 0) { _Head = 0; }
            else if (index < Count / 2)
            {
                // We had ACD___.Insert(1, B);
                // we move head back so we have _ACD__... <-- equivalent to popping tail and pushing head.
                _Head = GetOffset(-1);
                // we move everybody that needs to go west A_CD__..
                for (int i = index - 1; i >= 0; --i) this[i] = this[i + 1];
                // and then we'll set the index to meet our postcondition below.
            }
            else
            {
                // We had ABCE__.Insert(3, D)
                // All we need to do is move everybody that needs to go east a little, ABC_E_
                for (int i = index + 1; i < Count; ++i) this[i] = this[i - 1];
            }
            // Either way, we've made a gap for the new data to insert; stick it in!
            this[index] = item;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                int x = _Head + i;
                if (x >= Capacity) x -= Capacity;
                if (x >= Capacity) throw new IndexOutOfRangeException($"{i}<{Count} outside [0, {Capacity})");
                yield return _Data[x];
            }
        }
        public override void Clear()
        {
            for (int i = 0; i < Capacity; ++i) _Data[i] = default;
            _Head = 0;
            Count = 0;
        }
        public void EnsureCapacity(int capacity = 0)
        {
            if (capacity <= 0) capacity = System.Math.Max(16, 2 * Capacity);
            if (Limit > 0)
            {
                if (Capacity >= Limit && capacity > Limit) throw new NotSupportedException($"{Count} already >= {Limit}; can't grow to {capacity}");
                capacity = System.Math.Min(capacity, Limit);
            }
            if (Capacity < capacity) Resize(capacity);
        }
        public void TrimExcess(int capacity = 0)
        {
            if (capacity <= 0) capacity = System.Math.Max(Count, System.Math.Min(Limit, Capacity / 2));
            if (Count < capacity && capacity < Capacity) Resize(capacity);
        }
    }
}