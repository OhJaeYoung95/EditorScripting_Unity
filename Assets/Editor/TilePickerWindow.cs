using UnityEngine;
using UnityEditor;

public class TilePickerWindow : EditorWindow
{
    public enum Scale
    {
        x1, 
        x2,
        x3, 
        x4, 
        x5,
    }

    private Scale scale = Scale.x5;
    private Vector2 scrollPosition;
    private Vector2Int selectTileIndex;
    private bool isRepaint = false;


    [MenuItem("Window/Tile Picker")]
    public static void Open()
    {
        var window = EditorWindow.GetWindow<TilePickerWindow>();
        window.isRepaint = true;
        window.selectTileIndex = Vector2Int.zero;
        TileMap.SelectedTileIndex = 0;

        var title = new GUIContent("Tile Picker");
        //title.text = "Title Picker";
        window.titleContent = title;
    }

    private void OnGUI()
    {

        if (Selection.activeGameObject == null)
            return;

        var tileMap = Selection.activeGameObject.GetComponent<TileMap>();
        if (tileMap == null)
            return;

        var texture2D = tileMap.texture2D;
        if (texture2D == null)
            return;

        scale = (Scale)EditorGUILayout.EnumPopup("Zoom", scale);
        var offset = new Vector2(10, 25);
        int scaleFactor = ((int)scale + 1);

        var rect = new Rect(0, 0, 
            texture2D.width * scaleFactor, texture2D.height * scaleFactor);

        var viewPort = new Rect(offset.x, offset.y, position.width - offset.x, position.height - offset.y);
        var contentRect = new Rect(0, 0, rect.width + offset.x, rect.height + offset.y);
        scrollPosition = GUI.BeginScrollView(viewPort, scrollPosition, contentRect);
        GUI.DrawTexture(rect, texture2D);
        GUI.EndScrollView();

        var boxTex = new Texture2D(1, 1);
        var color = Color.blue;
        color.a = 0.3f;
        boxTex.SetPixel(0, 0, color);
        boxTex.Apply();


        var style = new GUIStyle(GUI.skin.customStyles[0]);
        style.normal.background = boxTex;


        var scaledSize = tileMap.tileSize * scaleFactor;
        scaledSize += tileMap.tilePadding * scaleFactor;

        var currentEvent = Event.current;
        if(currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            var mousePos = currentEvent.mousePosition;
            mousePos.x += scrollPosition.x - offset.x;
            mousePos.y += scrollPosition.y - offset.y;


            var counts = new Vector2(texture2D.width / (tileMap.tileSize.x + tileMap.tilePadding.x)
                                   , texture2D.height / (tileMap.tileSize.y + tileMap.tilePadding.y));
            selectTileIndex.x = Mathf.FloorToInt(mousePos.x / scaledSize.x);
            selectTileIndex.y = Mathf.FloorToInt(mousePos.y / scaledSize.y);
            selectTileIndex.x = Mathf.Clamp(selectTileIndex.x, 0, (int)counts.x - 1);
            selectTileIndex.y = Mathf.Clamp(selectTileIndex.y, 0, (int)counts.y - 1);

            TileMap.SelectedTileIndex = selectTileIndex.y * (int)counts.x + selectTileIndex.x;

            isRepaint = true;
        }

        var highightRect = new Rect(selectTileIndex.x * scaledSize.x + offset.x, selectTileIndex.y * scaledSize.y + offset.y, scaledSize.x, scaledSize.y);
        GUI.Box(highightRect, "", style);


        if (isRepaint)
        {
            Repaint();
            isRepaint = false;
        }
    }
}
