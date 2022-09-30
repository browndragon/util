using BDUtil.Pubsub;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil
{
    public class ValueTest : MonoBehaviour
    {
        public Val<bool> SomeValue = new();
        [Expandable]
        public Topic SomeInlinedAssetFromOverThere;
    }
}