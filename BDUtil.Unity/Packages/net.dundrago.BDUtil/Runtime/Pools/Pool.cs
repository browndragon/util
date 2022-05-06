using UnityEngine;

namespace BDUtil
{
    [AddComponentMenu("UTI/Pool")]
    [Tooltip("A pool is store of disabled game objects")]
    public class Pool : BasePool
    {
        public override void EnableMe(GameObject me) => Objects.Remove(me);  // This is eager; it often isn't in the cache.
        public override void DisableMe(GameObject me) => Objects.Add(me);
    }
}