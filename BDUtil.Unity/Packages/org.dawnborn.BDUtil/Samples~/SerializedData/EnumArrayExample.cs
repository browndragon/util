using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil
{
    public class EnumArrayExample : MonoBehaviour
    {
        public enum Elements
        {
            Small,
            Big,
            Hot,
            Cold,
            Fast,
            Heavy,
            Dark,
            Turbulence,
            Time,
        }
        [Serializable]
        public struct Struct
        {
            public UnityEngine.Color Color;
            public UnityEngine.Sprite Sprite;
            public string[] NestedTags;
        }
        [SerializeField, SuppressMessage("IDE", "IDE0044"), SuppressMessage("IDE", "IDE0051"), SuppressMessage("IDE", "IDE0052")]
        EnumArray<Elements, Struct> Descriptions = new();
    }
}