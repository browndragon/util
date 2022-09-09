using UnityEngine;

namespace BDUtil.Pubsub
{
    /// Notifies once everyone's released.
    [CreateAssetMenu(menuName = "BDUtil/LockTopic")]
    public class LockTopic : ValueTopic<Lock>
    {
        public override Lock Value
        {
            set
            {
                bool wasLocked = Value;
                this.value = value;
                if (wasLocked && !value) Publish();
            }
        }
    }
}