using UnityEngine;
using UnityEditor;

public class NewTileMapMenu
{
    Editor edit;
    [MenuItem("GameObject/Tile Map")]
    public static void CreateTileMap()
    {
        var go = new GameObject("Tile Map");
        go.AddComponent<TileMap>();
    }
}
