using UnityEngine;

namespace BDUtil.Screen
{
    [AddComponentMenu("BDUtil/Drag3D")]
    /// If you don't use a rigidbody, you do not get trigger collisions (etc).
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Drag3D : DragND
    {
        new Rigidbody rigidbody;
        protected override void Awake() { base.Awake(); rigidbody = GetComponent<Rigidbody>(); }
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
