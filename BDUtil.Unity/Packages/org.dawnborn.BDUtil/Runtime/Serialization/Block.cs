using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// Code to treat lists as binary sort structures.
    /// All assume that the list has been sorted previously and maintain that invariant;
    /// expect crazy results if you mix Add or Insert operations...
    public static class Blocks
    {
        public interface IBlock { }
        public interface IBlock<T> : IBlock, IReadOnlyList<T> { }

        [Serializable]
        public struct B8<T> : IBlock<T>
        {
            [SerializeField]
            public T _0, _1, _2, _3, _4, _5, _6, _7;

            public T this[int index]
            {
                get => index switch
                {
                    0 => _0,
                    1 => _1,
                    2 => _2,
                    3 => _3,
                    4 => _4,
                    5 => _5,
                    6 => _6,
                    7 => _7,
                    _ => throw new IndexOutOfRangeException($"{index} !< 8"),
                };
                set => _ = index switch
                {
                    0 => _0 = value,
                    1 => _1 = value,
                    2 => _2 = value,
                    3 => _3 = value,
                    4 => _4 = value,
                    5 => _5 = value,
                    6 => _6 = value,
                    7 => _7 = value,
                    _ => throw new IndexOutOfRangeException($"{index} !< 8"),
                };
            }
            public int Count => 8;

            public IEnumerator<T> GetEnumerator()
            {
                yield return _0;
                yield return _1;
                yield return _2;
                yield return _3;
                yield return _4;
                yield return _5;
                yield return _6;
                yield return _7;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        [Serializable]
        public struct B32<T> : IBlock<T>
        {
            [SerializeField]
            public B8<T> _0, _1, _2, _3;
            public T this[int index]
            {
                get => System.Math.DivRem(index, 8, out int remainder) switch
                {
                    0 => _0[remainder],
                    1 => _1[remainder],
                    2 => _2[remainder],
                    3 => _3[remainder],
                    _ => throw new IndexOutOfRangeException($"{index} !< 32"),
                };
                set => _ = System.Math.DivRem(index, 8, out int remainder) switch
                {
                    0 => _0[remainder] = value,
                    1 => _1[remainder] = value,
                    2 => _2[remainder] = value,
                    3 => _3[remainder] = value,
                    _ => throw new IndexOutOfRangeException($"{index} !< 32"),
                };
            }

            public int Count => 32;

            public IEnumerator<T> GetEnumerator()
            {
                yield return _0._0;
                yield return _0._1;
                yield return _0._2;
                yield return _0._3;
                yield return _0._4;
                yield return _0._5;
                yield return _0._6;
                yield return _0._7;

                yield return _1._0;
                yield return _1._1;
                yield return _1._2;
                yield return _1._3;
                yield return _1._4;
                yield return _1._5;
                yield return _1._6;
                yield return _1._7;

                yield return _2._0;
                yield return _2._1;
                yield return _2._2;
                yield return _2._3;
                yield return _2._4;
                yield return _2._5;
                yield return _2._6;
                yield return _2._7;

                yield return _3._0;
                yield return _3._1;
                yield return _3._2;
                yield return _3._3;
                yield return _3._4;
                yield return _3._5;
                yield return _3._6;
                yield return _3._7;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}