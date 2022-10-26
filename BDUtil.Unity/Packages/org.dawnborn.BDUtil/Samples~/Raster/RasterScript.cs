using System.Collections;
using System.Collections.Generic;
using BDUtil.Math;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RasterScript : MonoBehaviour
{
    public Vector2 Scale;
    Vector2Int LastClick;
    Vector2Int LastHover;
    Texture2D Pixels;
    void Awake()
    {
        Pixels = Texture2D.whiteTexture;
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 pointF = Input.mousePosition;
        pointF.x /= Scale.x;
        pointF.y /= Scale.y;
        LastHover = new(Mathf.RoundToInt(pointF.x), Mathf.RoundToInt(pointF.y));
        if (Input.GetMouseButtonUp(0)) LastClick = LastHover;
    }
    void OnGUI()
    {
        Rect rect = default;
        rect.size = Scale;
        foreach (Vector2Int p in LastClick.Rasterized(LastHover))
        {
            rect.position = new(
                Scale.x * p.x,
                // Gui draws from top left, so we need to "invert".
                Screen.height - Scale.y * p.y
            );
            GUI.DrawTexture(rect, Pixels);
        }
    }
}
