using UnityEngine;

namespace BDUtil.Screen
{
    [AddComponentMenu("BDUtil/Drag2D")]
    /// If you don't use a rigidbody, you do not get trigger collisions (etc).
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Drag2D : DragND
    {
        new Rigidbody2D rigidbody;
        protected override void Awake() { base.Awake(); rigidbody = GetComponent<Rigidbody2D>(); }
        protected override Vector3 RBPos
        {
            get => rigidbody.position;
            set => rigidbody.position = value;
        }
        protected override Vector3 RBVeloc
        {
            get => rigidbody.velocity;
            set => rigidbody.velocity = value;
        }
    }
}
