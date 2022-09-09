using System;

namespace BDUtil
{
    /// Counters: Number of locks outstanding, etc.
    /// This can be driven to allow negative values, but it doesn't _really_.
    /// It's true when it's locked, and false when it's unlocked; += 1 makes it more locked, -=2 unlocks it twice, =default makes it unlocked.
    [Serializable]
    public readonly struct Lock
    {
        readonly int locks;
        private Lock(int locks) => this.locks = locks;
        public bool IsLocked => locks > 0;
        public void AndThrow() { if (IsLocked) throw new InvalidOperationException(); }
        public static Lock operator ++(Lock @lock) => new(@lock.locks + 1);
        public static Lock operator --(Lock @lock) => new(@lock.locks - 1);
        public static Lock operator -(Lock @lock) => new(-@lock.locks);
        public static Lock operator +(Lock a, Lock b) => new(a.locks + b.locks);
        public static Lock operator -(Lock a, Lock b) => new(a.locks - b.locks);
        public static implicit operator bool(Lock @lock) => @lock.IsLocked;
        public static implicit operator Lock(bool locked) => new(locked ? 1 : 0);
        public static explicit operator Lock(uint locks) => new((int)locks);
    }
}