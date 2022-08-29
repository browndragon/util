using System;
using UnityEngine;

namespace BDUtil
{
    public class SDTest : MonoBehaviour
    {
        public Map<string, float> StringToFloat = new();
        public Map<float, string> FloatToString = new();
        public MultiMap<string, string> ForwardNicks = new();
        public BiMap<string, string> TargetNicks = new();
        public BiMultiMap<string, string> AllNicks = new();
    }
}