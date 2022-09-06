using System;
using UnityEngine;

namespace BDUtil
{
    public class SDTest : MonoBehaviour
    {
        public Map<string, float> StringToFloat = new();
        public Map<float, string> FloatToString = new();
        public Map<string, Set<string>> ForwardNicks = new();
        public BiMap<string, string> TargetNicks = new();
    }
}