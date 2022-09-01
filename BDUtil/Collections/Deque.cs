using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    public interface IReadOnlyDeque<T> : IReadOnlyList<T> { }
    public interface IDeque<T> : IList<T>, IList
    {
        /// The largest Count can be without triggering a resize
        int Capacity { get; }
        bool PopFront(out T value);
        bool PopBack(out T value);
        void PushFront(T value);
        void PushBack(T value);
        IEnumerable<T> PopEnumerable();
    }
    public static class Deques
    {
        public static bool PopFront<T>(this IDeque<T> thiz) => thiz.PopFront(out T _);
        public static bool PopBack<T>(this IDeque<T> thiz) => thiz.PopBack(out T _);
        public static bool PushFrontOrPop<T>(this IDeque<T> thiz, T value, out T oldValue)
        {
            if (((ICollection<T>)thiz).Count < thiz.Capacity)
            {
                thiz.PushFront(value);
                return false.Let(oldValue = default);
            }
            thiz.PopBack(out oldValue).OrThrow();
            thiz.PushFront(value);
            return true;
        }
        public static bool PushBackOrPop<T>(this IDeque<T> thiz, T value, out T oldValue)
        {
            if (((ICollection<T>)thiz).Count < thiz.Capacity)
            {
                thiz.PushBack(value);
                return false.Let(oldValue = default);
            }
            thiz.PopFront(out oldValue).OrThrow();
            thiz.PushBack(value);
            return true;
        }
    }
    /// A queue/stack supporting O(N/2) insertion & deletion in the middle,
    /// O(1) insertion & deletion at the ends, and control over what to do when you overflow (resize or throw away data).
    public class Deque<T> : IDeque<T>, IReadOnlyDeque<T>
    {
        // public so it's natively unity serializable. You really shouldn't adjust these though!
        T[] _Data;
        int _Head = 0;

        /// Total data currently stored.
        public int Count { get; private set; }
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

        public T this[int i]
        {
            get => _Data[GetOffset(i.CheckRange(0, Count))];
            set => _Data[GetOffset(i.CheckRange(0, Count))] = value;
        }

        public int IndexOf(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; ++i) if (predicate(this[i])) return i;
            return -1;
        }
        public int IndexOf(T t, IEqualityComparer<T> comparer)
        {
            comparer ??= EqualityComparer<T>.Default;
            for (int i = 0; i < Count; ++i) if (comparer.Equals(t, this[i])) return i;
            return -1;
        }
        public int IndexOf(T t) => IndexOf(t, null);
        public bool Contains(T t) => IndexOf(t) > 0;

        /// Unconditionally add to end of list, discarding head if needed.
        /// You can also use TryPushFront/-Back to be more thoughtful.
        public void Add(T t) => PushBack(t);
        public bool Remove(T t)
        {
            int index = IndexOf(t);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }
        public void RemoveAt(int index) => RemoveAt(index, out T _);
        public void RemoveAt(int index, out T item)
        {
            item = this[index.CheckRange(0, Count)];
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
        public void Insert(int index, T item)
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
        public bool PopFront(out T value)
        {
            if (Count <= 0) return false.Let(value = default);
            RemoveAt(0, out value);
            return true;
        }
        public bool PopBack(out T value)
        {
            if (Count <= 0) return false.Let(value = default);
            RemoveAt(Count - 1, out value);
            return true;
        }
        public void PushFront(T value) => Insert(0, value);
        public void PushBack(T value) => Insert(Count, value);

        (int fromHead, int fromZero) Limits
        {
            get
            {
                int tail = _Head + Count;
                return Count <= 0
                    ? (0, 0)
                    : tail <= Capacity
                        ? (tail, 0)
                        : (Capacity, tail - Capacity);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            (int max1, int max2) = Limits;
            for (int i = _Head; i < max1; ++i) yield return _Data[i];
            for (int i = 0; i < max2; ++i) yield return _Data[i];
        }
        /// Runs (destructively) forever, popping off the front.
        public IEnumerable<T> PopEnumerable()
        { while (PopFront(out T t)) yield return t; }

        public void Clear()
        {
            (int max1, int max2) = Limits;
            for (int i = _Head; i < max1; ++i) _Data[i] = default;
            for (int i = 0; i < max2; ++i) _Data[i] = default;
            _Head = 0;
            Count = 0;
        }

        public void EnsureCapacity(int capacity = 0)
        {
            if (capacity <= 0) capacity = System.Math.Min(16, 2 * Capacity);
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

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Iter.WriteTo(this, array, arrayIndex);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList.Add(object value)
        {
            if (value is not T t) return -1;
            Add(t);
            return Count - 1;
        }
        bool IList.Contains(object value)
        {
            if (value is not T t) return false;
            return Contains(t);
        }
        int IList.IndexOf(object value)
        {
            if (value is not T t) return -1;
            return IndexOf(t);
        }
        void IList.Insert(int index, object value) => Insert(index, (T)value);
        void IList.Remove(object value)
        {
            if (value is not T t) return;
            Remove(t);
        }
        void ICollection.CopyTo(Array array, int index) => Iter.WriteTo(this, array, index);
        bool ICollection<T>.IsReadOnly => false;
        bool IList.IsReadOnly => false;
        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => false;
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }
    }
}