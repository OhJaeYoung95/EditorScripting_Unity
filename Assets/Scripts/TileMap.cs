using System;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public static int SelectedTileIndex { get; set; }

    public Vector2 mapSize = new Vector2(20, 10);
    public Texture2D texture2D;
    public Vector2 tileSize;
    public Vector2 tileOffset;
    public Vector2 tilePadding { get; set; } = new Vector2(2, 2);
    public Sprite[] sprites;
    public Vector2 gridSize;
    public int pixelsToUnits;

    public static string GetTileID(int r, int c)
    {
        return $"Tile (${r}, {c})";
    }

    private void OnDrawGizmosSelected()
    {
        if (texture2D == null)
            return;

        var pos = transform.position;


        Gizmos.color = Color.gray;
        //var firstGridPos = new Vector2(pos.x + tileSize.x * 0.5f, pos.y - tileSize.y * 0.5f);

        //for (int y = 0; y < mapSize.y; ++y)
        //{
        //    for (int x = 0; x < mapSize.x; ++x)
        //        Gizmos.DrawWireCube(firstGridPos + new Vector2(tileSize.x * x, -tileSize.y * y), tileSize);
        //}

        for (int i = 1; i < mapSize.x; ++i)
        {
            var x = pos.x + tileSize.x * i;
            Gizmos.DrawLine(new Vector2(x, pos.y), new Vector2(x, pos.y - gridSize.y));
        }
        for (int i = 1; i < mapSize.y; ++i)
        {
            var y = pos.y - tileSize.y * i;
            Gizmos.DrawLine(new Vector2(pos.x, y), new Vector2(pos.x + gridSize.x, y));
        }

        var cen = new Vector2(pos.x + gridSize.x * 0.5f, pos.y - gridSize.y * 0.5f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(cen, gridSize);
    }
}
