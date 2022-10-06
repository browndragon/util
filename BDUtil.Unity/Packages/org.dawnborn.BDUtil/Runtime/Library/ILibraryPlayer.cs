
using System.Collections;
using BDUtil.Screen;
using UnityEngine;

namespace BDUtil.Library
{
    // The player's contract with libraries.
    public interface ILibraryPlayer : Snapshots.IFuzzControls
    {
        Camera camera { get; }
        Transform transform { get; }
        SpriteRenderer renderer { get; }
        AudioSource audio { get; }
        Coroutine StartCoroutine(IEnumerator enumerator);
    }
}