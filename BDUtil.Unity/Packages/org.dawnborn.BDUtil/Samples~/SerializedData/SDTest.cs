using System;
using System.Collections.Generic;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    public class SDTest : MonoBehaviour
    {
        public StoreMap<string, float> StringToFloat = new();
        public StoreMap<float, string> FloatToString = new();
        public StoreMap<string, Store<HashSet<string>, string>> ForwardNicks = new();
        public StoreMap<Raw.Bi.Map<string, string>, string, string> TargetNicks = new();
    }
}