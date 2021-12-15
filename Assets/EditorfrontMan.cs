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

    [SerializeField] editorTools currentTool;

    [SerializeField] List<PerformedEdit> ActionsToUndo;

    [SerializeField] bool ToggleOrSet = false;

    [SerializeField] bool AddOrClear = false;  //IF TOGGLEORSET IS SET TO SET (lol set to set) THEN HOLDING SHIFT WILL AFFECT WEATHER IT CLEARS OR ADDS 

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
        }
        else if (attachedGrid)          //IF LEFT CLICK IS RELEASED, PERFORM THE ATTACHED GRID ACTION OF THE SELECTED TOOL
        {
            Vector2Int gridSize = attachedGrid.size.Floor();

            Vector2Int bottomLeft = ((attachGridStart + ((MWP5050 - attachGridStart) / 2)) - (gridSize / 2)).Floor();

            HashSet<Vector2Int> selectedTiles = new HashSet<Vector2Int>();
            for (int x = bottomLeft.x; x < bottomLeft.x + gridSize.x; x++)
                for (int y = bottomLeft.y; y < bottomLeft.y + gridSize.y; y++)
                    selectedTiles.Add(new Vector2Int(x, y));

            List<Vector2Int> AffectedTiles = new List<Vector2Int>();  //KEEPS A LIST OF TILES THAT ACTUALLY GET AFFTECTED BY THE ACTION.  i.e. TILES WITH MINES WILL NOT BE AFFECTED BY THE SET MINE FUNCTION, AND THIS INFO IS NEEDED FOR UNDO PURPOSES

            foreach (Vector2Int tile in selectedTiles)
                if (PerformToolAction(tile)) AffectedTiles.Add(tile);

            Destroy(attachedGrid);


            //AFTER PERFORMING THE ACTION, ADD IT TO THE LIST OF THINGS TO UNDO
            ActionsToUndo.Add(new PerformedEdit(AffectedTiles));
        }

        if (Input.GetMouseButtonDown(1))  //RIGHT CLICK TO CANCEL
        {
            if (attachedGrid)
            {
                Destroy(attachedGrid);
            }
        }
        

        if (attachedGrid)//IF THERE IS IN ATTACHED GRID, MAKE INTO A SQUARE BETWEEN THE START POSITION AND THE MOUSE
        {
            attachedGrid.transform.position = attachGridStart + ((MWP5050 - attachGridStart) / 2);
            attachedGrid.size = Vector2.one + (MWP5050 - attachGridStart).Absolute();
        }

        if (Input.GetKeyDown(KeyCode.Q)){//Q SELECTS THE MINE TOOL
            currentTool = editorTools.mine;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {//E SELECTS THE DESTROY TOOL
            currentTool = editorTools.destroy;
            attachedGrid.sprite = destroyToggleGrid.sprite;  //testing mid selection change
        }
        if (Input.GetKeyDown(KeyCode.R))
        {//R SELECTS THE CREATE TOOL
            currentTool = editorTools.create;
            attachedGrid.sprite = createToggleGrid.sprite;  //testing mid selection change
        }
        if (Input.GetKeyDown(KeyCode.F))
        {//F SELECTS THE REVEAL TOOL
            currentTool = editorTools.reveal;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {//C SELECTS THE FLAG TOOL
            currentTool = editorTools.flag;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {//Z SELECTS UNDO ------- this is gonna be a bitch and a half to impliment
            currentTool = editorTools.mine;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {//TAB - CHANGES BETWEEN SETTING OR TOGGLING THE CURRENT ACTION -- destroy/create are unaffected and are always considred a set action
            ToggleOrSet = !ToggleOrSet;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {//SHIFT - WHILE HOLDING, CHANGES FROM ADDING A SET TO CLEARING A SET
            AddOrClear = true;
        }
        else AddOrClear = false;

    }

    public bool ToggleMineInTile(Vector2Int vectInt)
    {
        if (NewEditorTile.TilePosits.Contains(vectInt)) return NewEditorTile.NETs[vectInt].ToggleMine();
        return false;
    }

    public bool SetMineInTile(Vector2Int vectInt, bool AoC)
    {
        if (NewEditorTile.TilePosits.Contains(vectInt)) return NewEditorTile.NETs[vectInt].SetMine(AoC);
        return false;
    }

    public bool ToggleFlagInTile(Vector2Int vectInt)
    {
        if (NewEditorTile.TilePosits.Contains(vectInt)) return NewEditorTile.NETs[vectInt].ToggleFlag();
        return false;

    }

    public bool SetFlagInTile(Vector2Int vectInt, bool AoC)
    {
        if (NewEditorTile.TilePosits.Contains(vectInt)) return NewEditorTile.NETs[vectInt].SetFlag(AoC);
        return false;

    }

    public bool ToggleCoverInTile(Vector2Int vectInt)
    {
        if (NewEditorTile.TilePosits.Contains(vectInt)) return NewEditorTile.NETs[vectInt].ToggleCover();
        return false;

    }

    public bool SetCoverInTile(Vector2Int vectInt, bool AoC)
    {
        if (NewEditorTile.TilePosits.Contains(vectInt)) return NewEditorTile.NETs[vectInt].SetCover(AoC);
        return false;

    }

    public bool CreateTile(Vector2Int vectInt)  //DIFFERENT FROM JUST SPAWNING, THIS FUNCTION ALONE HAS THE POWER TO OVERWRITE THE DELETEDTILE LIST
    {
        if (!NewEditorTile.TilePosits.Contains(vectInt))
        {
            DeletedTiles.Remove(vectInt);
            SpawnEditorTile(vectInt);
            return true;
        }
        return false;
    }
    public bool DeleteTile(Vector2Int vectInt)
    {
        if (NewEditorTile.TilePosits.Contains(vectInt))
        {
            DeletedTiles.Add(vectInt);
            Destroy(NewEditorTile.NETs[vectInt]);
            NewEditorTile.TilePosits.Remove(vectInt);
            NewEditorTile.NETs.Remove(vectInt);
            return true;
        }
        return false;
    }

    bool PerformToolAction(Vector2Int vInt)
    {
        switch (currentTool)
        {
            case editorTools.mine:
                if(ToggleOrSet) return ToggleMineInTile(vInt);
                return SetMineInTile(vInt, AddOrClear);
                break;
            case editorTools.flag:
                if (ToggleOrSet) return ToggleFlagInTile(vInt);
                return SetFlagInTile(vInt, AddOrClear);
                break;
            case editorTools.reveal:
                if (ToggleOrSet) return ToggleCoverInTile(vInt);
                return SetCoverInTile(vInt, AddOrClear);
                break;
            case editorTools.create:
                return CreateTile(vInt);
                break;
            case editorTools.destroy:
                return DeleteTile(vInt);
                break;
        }
        print("selection performance error");
        return false;
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
    

    public enum editorTools
    {
        mine,
        flag,
        reveal,
        create,
        destroy,
    }

    class PerformedEdit
    {
        public editorTools usedTool;
        List<Vector2Int> affectedTiles;
        bool ToggledOrSet;
        bool AddedOrCleared;
        Action InverseOperation;

        public Vector2Int bottomLeftCoord;
        public Vector2Int sizeOfArea;

        public PerformedEdit(List<Vector2Int> AffectedTiles)
        {
            usedTool = EFM.currentTool;
            ToggledOrSet = EFM.ToggleOrSet;
            AddedOrCleared = EFM.AddOrClear;
            switch (usedTool)
            {
                case editorTools.mine:
                    if (ToggledOrSet) InverseOperation = () => { foreach (Vector2Int tile in affectedTiles) EFM.ToggleMineInTile(tile); };
                    break;
                case editorTools.flag:
                    break;
                case editorTools.reveal:
                    break;
                case editorTools.create:
                    break;
                case editorTools.destroy:
                    break;
            }
            affectedTiles = AffectedTiles;
        }
    }
}
