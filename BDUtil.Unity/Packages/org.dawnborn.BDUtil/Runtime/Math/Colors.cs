using System;
using UnityEngine;

namespace BDUtil.Math
{
    public static class Colors
    {
        [Flags]
        public enum Overrides
        {
            None = default,
            RH = 1 << 0,
            GS = 1 << 1,
            BV = 1 << 2,
            A = 1 << 3,
            AsHSVA = 1 << 4,
        }
        public static void Override(ref this Color thiz, Overrides overrideFields, Color overrides)
        {
            if (overrideFields.HasFlag(Overrides.AsHSVA))
            {
                HSVA thizz = thiz;
                thizz.Override(overrideFields, (HSVA)overrides);
                thiz = thizz;
                return;
            }
            thiz.r = overrideFields.HasFlag(Overrides.RH) ? overrides.r : thiz.r;
            thiz.g = overrideFields.HasFlag(Overrides.GS) ? overrides.g : thiz.g;
            thiz.b = overrideFields.HasFlag(Overrides.BV) ? overrides.b : thiz.b;
            thiz.a = overrideFields.HasFlag(Overrides.A) ? overrides.a : thiz.a;
        }
        public static void Override(ref this HSVA thiz, Overrides overrideFields, HSVA overrides)
        {
            if (!overrideFields.HasFlag(Overrides.AsHSVA))
            {
                Color thizz = thiz;
                thizz.Override(overrideFields, (Color)overrides);
                thiz = thizz;
                return;
            }
            thiz.h = overrideFields.HasFlag(Overrides.RH) ? overrides.h : thiz.h;
            thiz.s = overrideFields.HasFlag(Overrides.GS) ? overrides.s : thiz.s;
            thiz.v = overrideFields.HasFlag(Overrides.BV) ? overrides.v : thiz.v;
            thiz.a = overrideFields.HasFlag(Overrides.A) ? overrides.a : thiz.a;
        }
    }
}