using BDUtil.Math;
using UnityEngine;
namespace BDUtil.Library
{
    [Tooltip("Randomizes 'stuff' about a monobehaviour onEnable, and undoes it afterwards.")]
    [AddComponentMenu("BDUtil/Library/Randomize")]
    public class Randomize : MonoBehaviour
    {
        public Vector3 Scale;
        public float RotZ;
        public Vector2 InitialV;
        public Vector2 RelativeV;
        public HSVA HSVA;
        readonly Disposes.All onDisable = new();
        protected void OnEnable()
        {
            if (RotZ > 0)
            {
                Vector3 orig = transform.eulerAngles;
                Vector3 euler = orig;
                euler.z += Fuzz.Float(RotZ * 180f);
                euler.z = Mathf.Repeat(euler.z, 360f);
                transform.eulerAngles = euler;
                onDisable.Add(() => transform.eulerAngles = orig);
            }
            if (!Scale.HasNaN())
            {
                Vector3 orig = transform.localScale;
                Vector3 localScale = orig;
                localScale = Fuzz.Vector3(localScale);
                transform.localScale = localScale;
                onDisable.Add(() => transform.localScale = orig);
            }
            if (!InitialV.HasNaN() && !RelativeV.HasNaN())
            {
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb)
                {
                    Vector2 orig = rb.velocity;
                    rb.AddRelativeForce(InitialV + Fuzz.Vector2(RelativeV), ForceMode2D.Impulse);
                    onDisable.Add(() => rb.velocity = orig);
                }
            }
            if (
                !float.IsNaN(HSVA.h)
             && !float.IsNaN(HSVA.s)
             && !float.IsNaN(HSVA.v)
             && !float.IsNaN(HSVA.a)
            )
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr)
                {
                    Color orig = sr.color;
                    sr.color = (HSVA)orig + Fuzz.HSVA(HSVA);
                    onDisable.Add(() => sr.color = orig);
                }
            }
        }
        protected void OnDisable() => onDisable.Dispose();

    }
}