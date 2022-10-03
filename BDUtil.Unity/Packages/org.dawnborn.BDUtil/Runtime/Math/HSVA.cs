using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BDUtil.Math
{
    /// HSVA-colorspace color. Useful for generating/passing around random color information.
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct HSVA : IEquatable<HSVA>
    {
        public static readonly HSVA red = Color.red;
        // Solid green. RGBA is (0, 1, 0, 1).
        public static readonly HSVA green = Color.green;
        // Solid blue. RGBA is (0, 0, 1, 1).
        public static readonly HSVA blue = Color.blue;
        // Solid white. RGBA is (1, 1, 1, 1).
        public static readonly HSVA white = Color.white;
        // Solid black. RGBA is (0, 0, 0, 1).
        public static readonly HSVA black = Color.black;
        // Yellow. RGBA is (1, 0.92, 0.016, 1), but the color is nice to look at!
        public static readonly HSVA yellow = Color.yellow;
        // Cyan. RGBA is (0, 1, 1, 1).
        public static readonly HSVA cyan = Color.cyan;
        // Magenta. RGBA is (1, 0, 1, 1).
        public static readonly HSVA magenta = Color.magenta;
        // Gray. RGBA is (0.5, 0.5, 0.5, 1).
        public static readonly HSVA gray = Color.gray;
        // English spelling for ::ref::gray. RGBA is the same (0.5, 0.5, 0.5, 1).
        public static readonly HSVA grey = Color.grey;
        // Completely transparent. RGBA is (0, 0, 0, 0).
        public static readonly HSVA clear = Color.clear;

        public float h, s, v, a;

        // Access the r, g, b,a components using [0], [1], [2], [3] respectively.
        public float this[int index]
        {
            get => index switch
            {
                0 => h,
                1 => s,
                2 => v,
                3 => a,
                _ => throw new IndexOutOfRangeException("Invalid HSVA index(" + index + ")!"),
            };
            set => _ = index switch
            {
                0 => h = value,
                1 => s = value,
                2 => v = value,
                3 => a = value,
                _ => throw new IndexOutOfRangeException("Invalid Color index(" + index + ")!"),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HSVA(float _h, float _s, float _v, float _a)
        {
            h = _h; s = _s; v = _v; a = _a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HSVA(float _h, float _s, float _v)
        : this(_h, _s, _v, 1f) { }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA RGBToHSV(Color color)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            return new(h, s, v, color.a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator HSVA(Color c) => RGBToHSV(c);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Color(HSVA hsva) => Color.HSVToRGB(hsva.h, hsva.s, hsva.v).WithA(hsva.a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(HSVA other)
        => h == other.h
        && s == other.s
        && v == other.v
        && a == other.a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(HSVA a, HSVA b) => a.Equals(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(HSVA a, HSVA b) => !a.Equals(b);

        // Adds two HSVAs together. Each component is added separately.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA operator +(HSVA a, HSVA b) { return new HSVA(a.h + b.h, a.s + b.s, a.v + b.v, a.a + b.a); }

        // Subtracts HSVA /b/ from HSVA /a/. Each component is subtracted separately.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA operator -(HSVA a, HSVA b) { return new HSVA(a.h - b.h, a.s - b.s, a.v - b.v, a.a - b.a); }

        // Multiplies two HSVAs together. Each component is multiplied separately.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA operator *(HSVA a, HSVA b) { return new HSVA(a.h * b.h, a.s * b.s, a.v * b.v, a.a * b.a); }

        // Multiplies HSVA /a/ by the float /b/. Each HSVA component is scaled separately.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA operator *(HSVA a, float b) { return new HSVA(a.h * b, a.s * b, a.v * b, a.a * b); }

        // Multiplies HSVA /a/ by the float /b/. Each HSVA component is scaled separately.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA operator *(float b, HSVA a) { return new HSVA(a.h * b, a.s * b, a.v * b, a.a * b); }

        // Divides HSVA /a/ by the float /b/. Each HSVA component is scaled separately.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA operator /(HSVA a, float b) { return new HSVA(a.h / b, a.s / b, a.v / b, a.a / b); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is HSVA hsva && Equals(hsva);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Chain.Hash ^ h ^ s ^ v ^ a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"HSVA({h}, {s}, {v}, {a})";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA Lerp(HSVA a, HSVA b, float t)
        {
            t = Mathf.Clamp01(t);
            return new HSVA(
                a.h + (b.h - a.h) * t,
                a.s + (b.s - a.s) * t,
                a.v + (b.v - a.v) * t,
                a.a + (b.a - a.a) * t
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HSVA LerpUnclamped(HSVA a, HSVA b, float t)
        {
            return new HSVA(
                a.h + (b.h - a.h) * t,
                a.s + (b.s - a.s) * t,
                a.v + (b.v - a.v) * t,
                a.a + (b.a - a.a) * t
            );
        }
    }
}