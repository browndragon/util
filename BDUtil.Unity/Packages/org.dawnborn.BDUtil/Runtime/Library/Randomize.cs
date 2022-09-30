using BDUtil.Math;
using UnityEngine;
namespace BDUtil.Library
{
    [Tooltip("Randomizes 'stuff' about a monobehaviour onEnable, and undoes it afterwards.")]
    [AddComponentMenu("BDUtil/Library/Randomize")]
    public class Randomize : MonoBehaviour
    {
        [
            MinMax.Range(
                Display = MinMax.RangeAttribute.Displays.LogSlider,
                Min = .1f,
                Max = 10f
            )
        ]
        public MinMax Scale;
        [MinMax.Range(Min = -1f, Max = +1f)] public MinMax RotZ;
        public ColorRange Color;
        public MinMax Speed;
        readonly Disposes.All onDisable = new();
        protected void OnEnable()
        {
            if (RotZ.IsValid)
            {
                float rotation = RotZ.Random;
                Vector3 orig = transform.eulerAngles;
                Vector3 euler = orig;
                euler.z += rotation * 180f;
                euler.z = Mathf.Repeat(euler.z, 360f);
                transform.eulerAngles = euler;
                onDisable.Add(() => transform.eulerAngles = orig);
            }
            if (Scale.IsValid)
            {

                Vector3 orig = transform.localScale;
                Vector3 localScale = orig;
                localScale *= Scale.Random;
                transform.localScale = localScale;
                onDisable.Add(() => transform.localScale = orig);
            }
            if (Speed.IsValid)
            {
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb)
                {
                    Vector2 orig = rb.velocity;
                    rb.AddRelativeForce(Speed.Random * Vector2.up, ForceMode2D.Impulse);
                    onDisable.Add(() => rb.velocity = orig);
                }
            }
            ColorRange.HSVA hsva = Color;
            if (hsva.IsValid)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr)
                {
                    Color orig = sr.color;
                    sr.color = hsva.Random;
                    onDisable.Add(() => sr.color = orig);
                }
            }
        }
        protected void OnDisable() => onDisable.Dispose();

    }
}