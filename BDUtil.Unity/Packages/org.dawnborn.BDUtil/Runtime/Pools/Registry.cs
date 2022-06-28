using System.Linq;
using UnityEngine;

namespace BDUtil
{
    [AddComponentMenu("BDUtil/Registry")]
    [Tooltip("A registry is store of enabled game objects")]
    public class Registry : BasePool
    {
        public override void EnableMe(GameObject me) => Objects.Add(me);
        public override void DisableMe(GameObject me)
        {
            if (!Objects.Remove(me)) Debug.LogWarning(
                $"Registry failed to remove {me.GetInstanceID()} from {Objects.Keys.Summarize(5)}", me
            );
        }
    }
}