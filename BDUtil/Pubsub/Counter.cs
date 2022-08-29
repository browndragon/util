using System;
using System.Collections;

namespace BDUtil.Pubsub
{
    /// Easy synchronization point: yields while count > 0.
    /// Could be extended: Begin/End could take caller location and trace them!
    /// This complies with Unity's requirements for a yield instruction, too...
    [Serializable]
    public class Counter : IEnumerable, IEnumerator
    {
        int count;
        // Useful in the context of a unity yield or something; continuously yields this value
        // until count <= 0.
        public object Current = null;
        public int Count => count;
        public bool IsRunning => Count > 0;
        public bool IsCanceled
        {
            get => count == int.MinValue;
            set => count = value ? int.MinValue : 0;
        }
        public Counter() : base() { }
        public void Begin(int c = 1) => count += (count == int.MinValue) ? 0 : c;
        public void End(int c = 1) => count -= (count == int.MinValue) ? 0 : c;
        public void Reset() { Current = null; count = 0; }
        public Counter GetEnumerator() => this;
        object IEnumerator.Current => Current;
        bool IEnumerator.MoveNext() => IsRunning;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}