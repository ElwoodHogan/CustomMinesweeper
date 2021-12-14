using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static FrontMan;

public class EditorfrontMan : MonoBehaviour
{
    public int height;
    public int width;
    [SerializeField] int mines;

    public HashSet<Vector2Int> ValidTiles;  //these positions show where the NewEditorTile can spread to
    public NewEditorTile NET;
    public HashSet<Vector2Int> mineLocations;
    public HashSet<Vector2Int> flags;
    public HashSet<Vector2Int> DeletedTiles;

    public Transform EditorTileParent;
    public List<Tile> tiles;
    public static EditorfrontMan EFM;
    public int TilesRevealed = 0;
    public int TotalFlagged = 0;
    public int TotalTiles = 0;

    [SerializeField] SpriteRenderer mineToggleGrid;
    [SerializeField] SpriteRenderer flagToggleGrid;
    [SerializeField] SpriteRenderer createToggleGrid;
    [SerializeField] SpriteRenderer destroyToggleGrid;

    [SerializeField] Vector2 attachGridStart;
    [SerializeField] SpriteRenderer attachedGrid;

    public bool editing = false;  //Dictates weather the player is in-editor or not

    public SpriteRenderer grid;

    private void Awake()
    {
        EFM = this;
    }

    [Button]
    void SetBoard()
    {
        SetBoard(height, width, mines);
    }

    public void SetBoard(int _height, int _width, int _mines)
    {
        height = _height;
        width = _width;
        mines = _mines;
        TotalTiles = height * width;
        flags = new HashSet<Vector2Int>();
        mineLocations = new HashSet<Vector2Int>();
        DeletedTiles = new HashSet<Vector2Int>();
        ValidTiles = new HashSet<Vector2Int>();
        for (int x = 0; x <= width -1; x++)
        {
            for (int y = 0; y <= height -1; y++)
            {
                ValidTiles.Add(new Vector2Int(x, y));
            }
        }

        if (EditorTileParent)EditorTileParent.gameObject.SetActive(false);
        EditorTileParent = new GameObject("NewEditorTileParent").transform;
        EditorTileParent.gameObject.SetActive(true);

        Camera.main.transform.position = new Vector3((float)width / 2, (float)height / 2, -10);
        Camera.main.orthographicSize = Mathf.Clamp((Mathf.Max(height, width) / 2) + 3, 2, SettingMenuAI.SM.GetCC() ? 20 : 999);  //Scales the camera to envolope larger maps

        grid.enabled = true;

        InGameMenuAI.IGM.Init(height, width, mines);

        editing = true;

        SpawnEditorTile(new Vector2Int(width / 2, height / 2));
    }

    private void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int MWPFloored = mouseWorldPos.Floor();
        Vector2Int MWP2 = MWPFloored.V3toV2Int();
        Vector2 MWP5050 = new Vector2(MWPFloored.x+.5f, MWPFloored.y + .5f);

        if (!editing) return;

        //TODO: HAVE THE ATTACHED GRID BE ASSIGNED AN ACTION, I.E. TOGGLE MINE, TOGGLE FLAG, AND HAVE IT BE ABLE TO SET RATHER THAN JUST TOGGLE
        //MAYBE USE AN ACTION MAN IDK

        if (Input.GetMouseButton(0))    //WHILE HOLDING LEFT CLICK
        {
            if (!attachedGrid) {
                attachedGrid = Instantiate(mineToggleGrid, MWP5050, Quaternion.identity, EditorTileParent);
                attachGridStart = MWP5050;
            }
            attachedGrid.transform.position = attachGridStart + ((MWP5050-attachGridStart)/2);
            attachedGrid.size = Vector2.one + (MWP5050 - attachGridStart).Absolute();
        }
        else if(attachedGrid)
        {
            Vector2Int gridSize = attachedGrid.size.Floor();

            Vector2Int bottomLeft = ((attachGridStart + ((MWP5050 - attachGridStart) / 2)) - (gridSize / 2)).Floor();

            HashSet<Vector2Int> selectedTiles = new HashSet<Vector2Int>();
            for (int x = bottomLeft.x; x < bottomLeft.x + gridSize.x; x++)
                for (int y = bottomLeft.y; y < bottomLeft.y + gridSize.y; y++)
                    selectedTiles.Add(new Vector2Int(x, y));

            foreach (Vector2Int tile in selectedTiles)
                if (NewEditorTile.TilePosits.Contains(tile)) NewEditorTile.NETs[tile].ToggleMine();

            Destroy(attachedGrid);
        }


        /*
        if (Input.GetMouseButtonDown(0))//ON LEFT CLICK
        {
            if (DeletedTiles.Contains(MWPFloored2))  //IF CLICKING AN ALREADY DELETED TILE, SPAWN A NEW ONE
            {
                DeletedTiles.Remove(MWPFloored2);
                SpawnEditorTile(MWPFloored2);
            } else if (NewEditorTile.TilePosits.Contains(MWPFloored2))  //IF CLICKING A TILE, ADD A MINE TO IT
            {
                NewEditorTile.NETs[MWPFloored2].ToggleMine();
            }
            else//ELSE, THE PLAYER IS CLICKING OUTSIDE THE AREA, MEANING WE NEED TO ENLARGE THE END MAP SIZE, AND ALSO SPAWN A NEW TILE WHERE THEY CLICKED
            {
                ValidTiles.Add(MWPFloored2);
                SpawnEditorTile(MWPFloored2);
            }
        }else if (Input.GetMouseButtonDown(1))//ON RIGHT CLICK
        {

        }*/
    }

    public static void SpawnEditorTile(Vector2Int posit)
    {
        if (EFM.ValidTiles.Contains(posit) && !EFM.DeletedTiles.Contains(posit) && NewEditorTile.TilePosits.Add(posit))
            //IF THE POSITION IS VALID, AND ITS NOT A SPOT INTENTIONALLY DELETED, AND THERE IS NOT ALREADY A TILE THERE, SPAWN A TILE
        {
            NewEditorTile.TilePosits.Add(posit);
            NewEditorTile net = Instantiate(EFM.NET, posit.V2IntToV3(), Quaternion.identity, EFM.EditorTileParent);
            NewEditorTile.NETs.Add(posit, net);
        }
    }
}
