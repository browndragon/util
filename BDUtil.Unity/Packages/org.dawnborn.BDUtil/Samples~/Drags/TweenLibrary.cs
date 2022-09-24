using System;
using System.Collections;
using BDUtil;
using BDUtil.Library;
using UnityEngine;

public class TweenLibrary : Library<TweenLibrary.Tween>
{
    [Serializable]
    public struct Tween : Player.IPlayable
    {
        public Vector3 Euler;
        public float Duration;

        public float PlayOn(Player player)
        {
            SpriteRenderer target = player.GetComponent<SpriteRenderer>().OrThrow();
            player.StopAllCoroutines();
            Vector3 euler = target.transform.eulerAngles;
            Tween thiz = this;
            player.StartCoroutine(new Timer(thiz.Duration).@foreach(t
                => target.transform.eulerAngles = Vector3.Lerp(euler, thiz.Euler, t))
            );
            return Duration;
        }
    }
}
