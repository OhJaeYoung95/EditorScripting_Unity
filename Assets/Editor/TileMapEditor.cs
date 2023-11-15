using UnityEngine;
using UnityEditor;
using Unity.Burst.CompilerServices;
using System;
using Unity.VisualScripting;

[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor
{
    private TileMap tileMap;
    private TileBrush brush;
    private Vector3 mouseHitPos;

    private void OnEnable()
    {
        tileMap = target as TileMap;
        Tools.current = Tool.View;
        //UpdateTileMap();
        CreateBrush();
    }

    private void OnDisable()
    {
        tileMap = null;

        if (brush != null)
        {
            DestroyImmediate(brush.gameObject);
            //Destroy(brush.gameObject);
        }
    }
    private void UpdateTileMap()
    {
        if (tileMap.texture2D != null)
        {
            var path = AssetDatabase.GetAssetPath(tileMap.texture2D);
            var array = AssetDatabase.LoadAllAssetsAtPath(path);
            tileMap.sprites = new Sprite[array.Length - 1];
            for (int i = 1; i < array.Length; ++i)
            {
                tileMap.sprites[i - 1] = array[i] as Sprite;
            }

            //Array.Sort(tileMap.sprites, (x, y) => x.name.CompareTo(y.name));

            var sampleSprite = tileMap.sprites[0];
            var w = sampleSprite.textureRect.width;
            var h = sampleSprite.textureRect.height;

            tileMap.tileSize = new Vector2(w, h);
            tileMap.pixelsToUnits = (int)(w / sampleSprite.bounds.size.x);

            tileMap.gridSize = new Vector2(w * tileMap.mapSize.x, h * tileMap.mapSize.y);
            tileMap.gridSize /= tileMap.pixelsToUnits;

            ClearTiles();

            if (brush == null)
            {
                CreateBrush();
            }

            EditorUtility.SetDirty(tileMap);
        }
    }

    private void OnSceneGUI()
    {
        if (brush != null)
        {
            var sprite = tileMap.sprites[TileMap.SelectedTileIndex];
            if (brush.spriteRenderer.sprite != sprite)
            {
                brush.UpdateBrush(sprite);
            }

            UpdateHitPosition();
            MoveBrush();

            if (Event.current.shift)
            {
                Draw();
            }

            if (Event.current.control)
            {
                brush.brushColor = Color.red;
                Erase();
            }
            brush.brushColor = Color.blue;

        }
    }

    private void Erase()
    {
        if (brush == null)
            return;

        var tileId = TileMap.GetTileID(brush.gridIndex.y, brush.gridIndex.x);
        var tileTr = tileMap.transform.Find(tileId);

        if (tileTr != null)
        {
            DestroyImmediate(tileTr.gameObject);
        }

        EditorUtility.SetDirty(tileMap);
    }

    private void ClearTiles()
    {

        int count = tileMap.transform.childCount;
        for (int i = count - 1; i >= 0; --i)
        {
            var child = tileMap.transform.GetChild(i).gameObject;
            if (brush != null && child == brush.gameObject)
                continue;

            DestroyImmediate(child);
        }
    }

    private void MoveBrush()
    {
        var column = Mathf.FloorToInt(mouseHitPos.x / tileMap.tileSize.x);
        var row = -Mathf.FloorToInt(mouseHitPos.y / tileMap.tileSize.y) - 1;

        column = (int)Mathf.Clamp(column, 0, tileMap.mapSize.x - 1);
        row = (int)Mathf.Clamp(row, 0, tileMap.mapSize.y - 1);

        brush.gridIndex.x = column;
        brush.gridIndex.y = row;


        var x = column * tileMap.tileSize.x;
        var y = -(row + 1) * tileMap.tileSize.y;
        x += tileMap.tileSize.x * 0.5f;
        y += tileMap.tileSize.y * 0.5f;

        brush.transform.localPosition = new Vector3(x, y, tileMap.transform.position.z - 1f);
    }

    private void Draw()
    {
        if (brush == null)
            return;

        GameObject tileGo = null;
        var tileId = TileMap.GetTileID(brush.gridIndex.y, brush.gridIndex.x);
        var tileTr = tileMap.transform.Find(tileId);

        if (tileTr != null)
        {
            tileGo = tileTr.gameObject;
        }

        if (tileGo == null)
        {
            tileGo = new GameObject(tileId);
            tileGo.transform.SetParent(tileMap.transform);
            var pos = brush.transform.position;
            pos.z = tileMap.transform.position.z;

            tileGo.transform.position = pos;
            tileGo.AddComponent<SpriteRenderer>();
            //tileMap.tiles[brush.gridIndex.y, brush.gridIndex.x] = tileGo;
        }
        var ren = tileGo.GetComponent<SpriteRenderer>();
        ren.sprite = brush.spriteRenderer.sprite;
        EditorUtility.SetDirty(tileMap);
    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Our custom editor");
        var oldSize = tileMap.mapSize;
        tileMap.mapSize = EditorGUILayout.Vector2Field("Map Size", tileMap.mapSize);

        var oldTexture = tileMap.texture2D;
        tileMap.texture2D = EditorGUILayout.ObjectField("Texture2D: ", tileMap.texture2D, typeof(Texture2D), false) as Texture2D;

        if (oldSize != tileMap.mapSize || oldTexture != tileMap.texture2D)
            UpdateTileMap();

        if (tileMap.texture2D == null)
        {
            EditorGUILayout.HelpBox("텍스처를 설정하세요", MessageType.Warning);

        }
        else
        {
            EditorGUILayout.LabelField($"Tile Size: {tileMap.tileSize}");
            EditorGUILayout.LabelField($"Grid Size: {tileMap.gridSize}");
            EditorGUILayout.LabelField($"pixels To Units: {tileMap.pixelsToUnits}");
        }

        if (GUILayout.Button("Clear Tiles"))
        {
            if (EditorUtility.DisplayDialog("Clear map's tiles?", "Are you sure?", "Clear", "Do not clear"))
            {
                ClearTiles();
            }
        }

        EditorGUILayout.EndVertical();
        EditorUtility.SetDirty(tileMap);
    }

    private void CreateBrush()
    {
        if (brush != null)
        {
            DestroyImmediate(brush.gameObject);
        }

        if (tileMap.texture2D == null)
            return;

        var sprite = tileMap.sprites[TileMap.SelectedTileIndex];
        if (sprite == null)
            return;

        var newGo = new GameObject("Brush");
        newGo.transform.SetParent(tileMap.transform);
        brush = newGo.AddComponent<TileBrush>();
        brush.spriteRenderer = newGo.AddComponent<SpriteRenderer>();
        brush.brushSize = new Vector2(sprite.textureRect.width, sprite.textureRect.height);
        brush.brushSize /= tileMap.pixelsToUnits;

        brush.UpdateBrush(sprite);
    }

    private void UpdateHitPosition()
    {
        var mousePos = Event.current.mousePosition;
        var ray = HandleUtility.GUIPointToWorldRay(mousePos);
        var p = new Plane(tileMap.transform.TransformDirection(Vector3.forward), Vector3.zero);

        var distance = 0f;
        var hit = Vector3.zero;
        if (p.Raycast(ray, out distance))
        {
            hit = ray.origin + ray.direction.normalized * distance;
        }

        mouseHitPos = tileMap.transform.InverseTransformPoint(hit);
    }
}
