using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBrush : MonoBehaviour
{
    public Vector2 brushSize = Vector2.zero;
    public SpriteRenderer spriteRenderer;
    public Vector2Int gridIndex;

    public Color brushColor = Color.blue;

    private void OnDrawGizmos()
    {
        var color = brushColor;
        color.a = 0.5f;
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, brushSize);
    }

    public void UpdateBrush(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }
}
