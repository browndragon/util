using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil;

namespace BDUtil.Raw
{
    public interface IDeque<T> : IList<T>
    {
        bool PopFront(out T value);
        bool PopBack(out T value);
        void PushFront(T value);
        void PushBack(T value);
        IEnumerable<T> PopEnumerable();
    }
    /// A queue/stack supporting O(N/2) insertion & deletion in the middle,
    /// O(1) insertion & deletion at the ends, and control over what to do when you overflow (resize or throw away data).
    [Serializable]
    public abstract class BaseDeque<T> : IDeque<T>, IReadOnlyList<T>
    {
        // public so it's natively unity serializable. You really shouldn't adjust these though!
        public T[] _Data;
        public int _Count = 0;
        public int _Head = 0;
        int ICollection<T>.Count => _Count;
        int IReadOnlyCollection<T>.Count => _Count;
        /// Total amount this can hold before discarding data.
        public int Capacity => _Data?.Length ?? 0;
        int GetOffset(int? offset = default) => (_Head + (offset ?? _Count)).PosMod(Capacity);

        public BaseDeque(int size) => Resize(size);
        public void Resize(int newSize)
        {
            if (newSize < 0) newSize = 0;
            T[] newData = null;
            if (newSize > 0)
            {
                newData = new T[newSize];
                newSize = newData.Length;
                if (_Count < newSize) newSize = _Count;
                for (int i = 0; i < newSize; ++i) newData[i] = this[i];
            }
            _Data = newData;
            _Head = 0;
            _Count = newSize;
        }
        public void EnsureCapacity(int capacity) { if (Capacity < capacity) Resize(capacity); }
        public void EnsureCapacity() { if (_Count > Capacity / 2) EnsureCapacity(2 * Capacity); }

        public T this[int i]
        {
            get => _Data[GetOffset(i.CheckRange(0, _Count))];
            set => _Data[GetOffset(i.CheckRange(0, _Count))] = value;
        }

        public int IndexOf(Predicate<T> predicate)
        {
            for (int i = 0; i < _Count; ++i) if (predicate(this[i])) return i;
            return -1;
        }
        public int IndexOf(T t, IEqualityComparer<T> comparer)
        {
            comparer ??= EqualityComparer<T>.Default;
            for (int i = 0; i < _Count; ++i) if (comparer.Equals(t, this[i])) return i;
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
            item = this[index.CheckRange(0, _Count)];
            if (index >= _Count / 2) for (
                int i = index + 1; i < _Count; ++i
            ) this[i - 1] = this[i];
            else
            {
                for (int i = index - 1; i >= 0; --i) this[i + 1] = this[i];
                _Head = GetOffset(1);
            }
            _Count -= 1;
        }

        void IList<T>.Insert(int index, T item) => Insert(index, item);
        protected abstract void HandleOverCapacity(ref int index, T item, bool shiftLeftOnError);
        // Insert so that `this[index]==item` afterwards.
        public void Insert(int index, T item, bool shiftLeftOnError = false)
        {
            index.CheckRangeInclusive(0, _Count);
            if (_Count >= Capacity) HandleOverCapacity(ref index, item, shiftLeftOnError);

            // Ok, now make room to insert the element we're going to add.
            // the element is actually pushed back now (but default).
            if (_Count++ == 0) { _Head = 0; }
            else if (index < _Count / 2)
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
                for (int i = index + 1; i < _Count; ++i) this[i] = this[i - 1];
            }
            // Either way, we've made a gap for the new data to insert; stick it in!
            this[index] = item;
        }
        public bool PopFront(out T value)
        {
            if (_Count <= 0) return false.Let(value = default);
            RemoveAt(0, out value);
            return true;
        }
        public bool PopBack(out T value)
        {
            if (_Count <= 0) return false.Let(value = default);
            RemoveAt(_Count - 1, out value);
            return true;
        }
        public void PushFront(T value) => Insert(0, value);
        public void PushBack(T value) => Insert(_Count, value, true);

        (int fromHead, int fromZero) Limits
        {
            get
            {
                int tail = _Head + _Count;
                return _Count <= 0
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
        public IEnumerable<T> PopEnumerable()
        { while (PopFront(out T t)) yield return t; }

        public void Clear()
        {
            (int max1, int max2) = Limits;
            for (int i = _Head; i < max1; ++i) _Data[i] = default;
            for (int i = 0; i < max2; ++i) _Data[i] = default;
            _Head = 0;
            _Count = 0;
        }
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Iter.WriteTo(this, array, arrayIndex);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        bool ICollection<T>.IsReadOnly => false;
    }
    /// Attempts to overfill throw ArgException.
    public class DequeThrows<T> : BaseDeque<T>
    {
        public DequeThrows(int size) : base(size) { }
        protected override void HandleOverCapacity(ref int index, T item, bool shiftLeftOnError)
        => throw new ArgumentException($"this.Insert({index}, {item}, {(shiftLeftOnError ? "<<" : ">>")} over capacity {Capacity}");
    }
    /// Like a List; attempts to overfill grow the ring.
    public class DequeGrows<T> : BaseDeque<T>
    {
        public DequeGrows() : base(0) { }

        public void TrimExcess(int capacity) { if (_Count < capacity && Capacity < capacity) Resize(capacity); }
        public void TrimExcess() => TrimExcess(Capacity / 3);
        protected override void HandleOverCapacity(ref int index, T item, bool shiftLeftOnError)
        => EnsureCapacity();
    }
    /// A real ringbuffer; attempts to overfill pop data from the other side of the queue.
    public class DequePops<T> : BaseDeque<T>
    {
        public DequePops(int size) : base(size) { }
        protected override void HandleOverCapacity(ref int index, T item, bool shiftLeftOnError)
        => _ = shiftLeftOnError
        /// They lied to us!!! Say capacity/count are 4/4 (=ABCD) & we're inserting +E @count (=4).
        /// Even after the resize, we'll have BCD_ but still be trying to insert after the _!
        /// So we need to adjust the index.
        ? PopFront(out var _).Let(index--)
        : PopBack(out var _);
    }
}