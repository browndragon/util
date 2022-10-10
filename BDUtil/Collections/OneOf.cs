// using System;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;

// namespace BDUtil.Math
// {
//     [Serializable]
//     [StructLayout(LayoutKind.Explicit)]
//     public struct RawOneOf<T1, T2>
//     // where T1 : unmanaged
//     // where T2 : unmanaged
//     {
//         [FieldOffset(0)]
//         public T1 t1;
//         [FieldOffset(0)]
//         public T2 t2;
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public RawOneOf(T1 t1)
//         {
//             this = default;
//             this.t1 = t1;
//         }
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public RawOneOf(T2 t2)
//         {
//             this = default;
//             this.t2 = t2;
//         }
//     }
//     [Serializable]
//     public struct OneOf<T1, T2> : IEquatable<OneOf<T1, T2>>
//     // where T1 : unmanaged
//     // where T2 : unmanaged
//     {
//         public bool IsT2;
//         public RawOneOf<T1, T2> oneOf;
//         public static OneOf<T1, T2> Of1(T1 t1)
//         {
//             OneOf<T1, T2> thiz = default;
//             thiz._1 = t1;
//             return thiz;
//         }
//         public static OneOf<T1, T2> Of2(T2 t2)
//         {
//             OneOf<T1, T2> thiz = default;
//             thiz._2 = t2;
//             return thiz;
//         }
//         public T1 _1
//         {
//             get => !IsT2 ? oneOf.t1 : throw new InvalidCastException();
//             set
//             {
//                 IsT2 = false;
//                 oneOf = new(value);
//             }
//         }
//         public T2 _2
//         {
//             get => IsT2 ? oneOf.t2 : throw new InvalidCastException();
//             set
//             {
//                 IsT2 = true;
//                 oneOf = new(value);
//             }
//         }
//         public static explicit operator T1(OneOf<T1, T2> t12) => t12._1;
//         public static explicit operator T2(OneOf<T1, T2> t12) => t12._2;
//         public void Deconstruct(out bool isT2, out T1 t1, out T2 t2)
//         {
//             isT2 = IsT2;
//             t1 = IsT2 ? default : oneOf.t1;
//             t2 = IsT2 ? oneOf.t2 : default;
//         }
//         public T Switch<T>(Func<T1, T> ifT1, Func<T2, T> ifT2) => IsT2 ? ifT2.Invoke(oneOf.t2) : ifT1.Invoke(oneOf.t1);
//         public bool Equals(OneOf<T1, T2> other) => IsT2 == other.IsT2 && (IsT2 ? _2.Equals(other._2) : _1.Equals(other._1));
//         public override bool Equals(object obj) => obj is OneOf<T1, T2> other && Equals(other);
//         public override int GetHashCode() => Chain.Hash ^ IsT2.GetHashCode() ^ (IsT2 ? _2.GetHashCode() : _1.GetHashCode());
//         public override string ToString()
//         => !IsT2
//         ? $"<{typeof(T1)},_{typeof(T2)}_>({oneOf.t1})"
//         : $"<_{typeof(T1)}_,{typeof(T2)}>({oneOf.t2})";
//     }
// }
