using UnityEngine;

namespace BDUtil.Pubsub
{
    public class ScenePhysics2D : MonoBehaviour
    {
        public Vector2 Gravity;
        Vector2 hadGravity;
        protected void OnEnable()
        {
            hadGravity = Physics2D.gravity;
            Physics2D.gravity = Gravity;
        }
        protected void OnDisable()
        {
            Physics2D.gravity = hadGravity;
        }
    }
}