using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static FrontMan;

public class EditorfrontMan : MonoBehaviour
{
    public int height;
    public int width;
    public int mines;

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
    [SerializeField] SpriteRenderer revealToggleGrid;
    [SerializeField] SpriteRenderer createToggleGrid;
    [SerializeField] SpriteRenderer destroyToggleGrid;
    [SerializeField] SpriteRenderer addGridOverlay;
    [SerializeField] SpriteRenderer clearGridOverlay;

    [SerializeField] Vector2 attachGridStart;
    [SerializeField] SpriteRenderer attachedGrid;
    [SerializeField] SpriteRenderer overlayGrid;

    public editorTools currentTool;

    [SerializeField] List<PerformedEdit> ActionsToUndo;

    public bool ToggleOrSet = true;

    public bool AddOrClear = true;  //IF TOGGLEORSET IS SET TO SET (lol set to set) THEN HOLDING SHIFT WILL AFFECT WEATHER IT CLEARS OR ADDS 

    public bool editing = false;  //Dictates weather the player is in-editor or not

    public SpriteRenderer grid;

    bool usingStartSquare = false;
    public Vector2Int startSquarePos;
    public Transform StartSquare;

    public Transform hoveredTileIndicator;

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
        ActionsToUndo = new List<PerformedEdit>();
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

        //InEditorMenuAI.IEM.Init(height, width, mines);

        editing = true;

        SpawnEditorTile(new Vector2Int(width / 2, height / 2));

        hoveredTileIndicator.gameObject.SetActive(true);
    }

    bool destroyedGrid = false;//IF THE USER CANCELS THE ACTION, THIS PREVENTS A NEW GRID FROM SPAWNING ON THE MOUSE
    private void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int MWPFloored = mouseWorldPos.Floor();
        Vector2Int MWP2 = MWPFloored.V3toV2Int();
        Vector2 MWP5050 = new Vector2(MWPFloored.x+.5f, MWPFloored.y + .5f);

        if (!editing || EventSystem.current.IsPointerOverGameObject()) return;

        //TODO: HAVE THE ATTACHED GRID BE ASSIGNED AN ACTION, I.E. TOGGLE MINE, TOGGLE FLAG, AND HAVE IT BE ABLE TO SET RATHER THAN JUST TOGGLE
        //MAYBE USE AN ACTION MAN IDK

        if (Input.GetMouseButton(0))    //WHILE HOLDING LEFT CLICK
        {
            if (!attachedGrid && !destroyedGrid)
            {
                attachedGrid = Instantiate(GridSpriteOfTool(), MWP5050, Quaternion.identity, EditorTileParent);
                overlayGrid = Instantiate(AddOrClear ? addGridOverlay : clearGridOverlay, MWP5050, Quaternion.identity, EditorTileParent);
                if (ToggleOrSet) overlayGrid.enabled = false;
                attachGridStart = MWP5050;
            }

            SpriteRenderer GridSpriteOfTool()
            {
                switch (currentTool)
                {
                    case editorTools.mine:
                        return mineToggleGrid;
                    case editorTools.flag:
                        return flagToggleGrid;
                    case editorTools.reveal:
                        return revealToggleGrid;
                    case editorTools.create:
                        return createToggleGrid;
                    case editorTools.destroy:
                        return destroyToggleGrid;
                }
                print("grid selection error");
                return null;
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
            List<NewEditorTile.basicTileData> deletedTiles = new List<NewEditorTile.basicTileData>();//IF DELETING TILES, SAVE A LIST OF THEIR DATA IN THE CASE OF UNDOING THE DELETION

            if (currentTool == editorTools.destroy)
            {
                foreach (Vector2Int tile in selectedTiles) if (NewEditorTile.TilePosits.Contains(tile)) deletedTiles.Add(NewEditorTile.NETs[tile].GetBasicData());
            }

            foreach (Vector2Int tile in selectedTiles)
                if (PerformToolAction(tile)) AffectedTiles.Add(tile);

            Destroy(attachedGrid.gameObject);
            Destroy(overlayGrid.gameObject);

            //AFTER PERFORMING THE ACTION, ADD IT TO THE LIST OF THINGS TO UNDO
            if (AffectedTiles.Count > 0) ActionsToUndo.Add(new PerformedEdit(AffectedTiles, deletedTiles));

            //ALSO GET THE NEW Width/height COUNTS FOR CREATE AND DESTROY
            if(currentTool == editorTools.create || currentTool == editorTools.destroy)
            {
                int leftMostX = 99999;
                int rightMostX = -99999;
                int downMostY = 99999;
                int upMostY = -99999;
                foreach (var vect in NewEditorTile.TilePosits)
                {
                    if (vect.x < leftMostX) leftMostX = vect.x;
                    if (vect.x > rightMostX) rightMostX = vect.x;
                    if (vect.y < downMostY) downMostY = vect.y;
                    if (vect.y > upMostY) upMostY = vect.y;
                }
                width =  1 +rightMostX - leftMostX;
                height = 1 +upMostY - downMostY;
            }
            /*
            if(currentTool == editorTools.mine)
            {
                int m = 0;
                foreach (var tile in NewEditorTile.NETs)
                {
                    if (tile.Value.containsMine) m++;
                }
                mines = m;
            }*/
        }
        else
        {
            destroyedGrid = false;
        }

        if (Input.GetMouseButtonDown(1))  //RIGHT CLICK TO CANCEL
        {
            if (attachedGrid)
            {
                destroyedGrid = true;
                Destroy(attachedGrid.gameObject);
                Destroy(overlayGrid.gameObject);
            }
        }
        

        if (attachedGrid)//IF THERE IS IN ATTACHED GRID, MAKE INTO A SQUARE BETWEEN THE START POSITION AND THE MOUSE
        {
            attachedGrid.transform.position = attachGridStart + ((MWP5050 - attachGridStart) / 2);
            attachedGrid.size = Vector2.one + (MWP5050 - attachGridStart).Absolute();
            overlayGrid.transform.position = attachGridStart + ((MWP5050 - attachGridStart) / 2);
            overlayGrid.size = Vector2.one + (MWP5050 - attachGridStart).Absolute();
        }

        if (hoveredTileIndicator.gameObject.activeSelf)
        {
            hoveredTileIndicator.position = MWPFloored;
        }

        if (Input.GetKeyDown(KeyCode.Q)){//Q SELECTS THE MINE TOOL
            EquipMines();
        }

        if (Input.GetKeyDown(KeyCode.E)) {//E SELECTS THE DESTROY TOOL
            EquipDestroy();
        }

        if (Input.GetKeyDown(KeyCode.R)) {//R SELECTS THE CREATE TOOL
            EquipCreate();
        }

        if (Input.GetKeyDown(KeyCode.F))  {//F SELECTS THE REVEAL TOOL
            EquipReveal();
        }

        if (Input.GetKeyDown(KeyCode.C))    {//C SELECTS THE FLAG TOOL
            EquipFlag();
        }

        if (Input.GetKeyDown(KeyCode.Z))     {//CTRL+Z SELECTS UNDO ------- this is gonna be a bitch and a half to impliment
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand))
            {
                Undo();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftCommand))  {//this just lets the user press z then ctrl, rather than ctrl then z
            if (Input.GetKey(KeyCode.Z))
            {
                Undo();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!usingStartSquare)
            {
                StartSquare.gameObject.SetActive(true);
                StartSquare.position = MWPFloored;
                startSquarePos = MWP2;
                usingStartSquare = true;
            }
            else if(startSquarePos == MWP2)
            {
                usingStartSquare = false;
                StartSquare.gameObject.SetActive(false);
            }
            else
            {
                StartSquare.position = MWPFloored;
                startSquarePos = MWP2;
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))   {//TAB - CHANGES BETWEEN SETTING OR TOGGLING THE CURRENT ACTION -- destroy/create are unaffected and are always considred a set action
            SwitchToggleOrSet();
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {//SHIFT - WHILE HOLDING, CHANGES FROM ADDING A SET TO CLEARING A SET
            AddOrClear = false;
                if (overlayGrid) overlayGrid.sprite = AddOrClear ? addGridOverlay.sprite : clearGridOverlay.sprite;
        }
        else
        {
                AddOrClear = true;
                if (overlayGrid) overlayGrid.sprite = AddOrClear ? addGridOverlay.sprite : clearGridOverlay.sprite;
        }

    }

    #region switch Tools
    public void EquipMines()
    {
        currentTool = editorTools.mine;
        if (attachedGrid) attachedGrid.sprite = mineToggleGrid.sprite;
    }
    public void EquipFlag()
    {
        currentTool = editorTools.flag;
        if (attachedGrid) attachedGrid.sprite = flagToggleGrid.sprite;
    }
    public void EquipReveal()
    {
        currentTool = editorTools.reveal;
        if (attachedGrid) attachedGrid.sprite = revealToggleGrid.sprite;
    }
    public void EquipCreate()
    {
        currentTool = editorTools.create;
        if (attachedGrid) attachedGrid.sprite = createToggleGrid.sprite;
    }
    public void EquipDestroy()
    {
        currentTool = editorTools.destroy;
        if (attachedGrid) attachedGrid.sprite = destroyToggleGrid.sprite;
    }
    public void SwitchToggleOrSet()
    {
        ToggleOrSet = !ToggleOrSet;
        if (overlayGrid) overlayGrid.enabled = !ToggleOrSet;
    }
    public void Undo()
    {
        if (ActionsToUndo.Count > 0)
        {
            ActionsToUndo[ActionsToUndo.Count - 1].InverseOperation.Invoke();
            ActionsToUndo.RemoveAt(ActionsToUndo.Count - 1);
        }
    }
    #endregion

    #region Tile Change Functions
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

    public bool CreateTile(Vector2Int vectInt, bool setMine = false, bool setFlag = false, bool setCover = true, int setNearbyMine = 0)  //DIFFERENT FROM JUST SPAWNING, THIS FUNCTION ALONE HAS THE POWER TO OVERWRITE THE DELETEDTILE LIST
    {
        if (!NewEditorTile.TilePosits.Contains(vectInt))
        {
            DeletedTiles.Remove(vectInt);
            ValidTiles.Add(vectInt);
            SpawnEditorTile(vectInt, setMine, setFlag, setCover, setNearbyMine);
            return true;
        }
        return false;
    }
    public bool DeleteTile(Vector2Int vectInt)
    {
        if (NewEditorTile.TilePosits.Contains(vectInt))
        {
            DeletedTiles.Add(vectInt);
            Destroy(NewEditorTile.NETs[vectInt].gameObject);
            NewEditorTile.TilePosits.Remove(vectInt);
            NewEditorTile.NETs.Remove(vectInt);
            return true;
        }
        return false;
    }
    #endregion
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

    public void SpawnEditorTile(Vector2Int posit, bool setMine = false, bool setFlag = false, bool setCover = true, int setNearbyMine = 0)
    {
        if (ValidTiles.Contains(posit) && !DeletedTiles.Contains(posit) && NewEditorTile.TilePosits.Add(posit))
            //IF THE POSITION IS VALID, AND ITS NOT A SPOT INTENTIONALLY DELETED, AND THERE IS NOT ALREADY A TILE THERE, SPAWN A TILE
        {
            NewEditorTile.TilePosits.Add(posit);
            NewEditorTile net = Instantiate(NET, posit.V2IntToV3(), Quaternion.identity, EditorTileParent);
            net.Init(setMine, setFlag, setCover, setNearbyMine);
            NewEditorTile.NETs.Add(posit, net);
        }
    }

    public void DeactivateBoard()
    {
        EditorTileParent.gameObject.SetActive(false);
        hoveredTileIndicator.gameObject.SetActive(false);
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
        List<NewEditorTile.basicTileData> deletedTileDatas;
        bool ToggledOrSet;
        bool AddedOrCleared;
        public Action InverseOperation;

        public Vector2Int bottomLeftCoord;
        public Vector2Int sizeOfArea;

        public PerformedEdit(List<Vector2Int> AffectedTiles, List<NewEditorTile.basicTileData> tileDatas)
        {
            usedTool = EFM.currentTool;
            ToggledOrSet = EFM.ToggleOrSet;
            AddedOrCleared = EFM.AddOrClear;
            deletedTileDatas = new List<NewEditorTile.basicTileData>(tileDatas);
            switch (usedTool)
            {
                case editorTools.mine:
                    if (ToggledOrSet) InverseOperation = () => { foreach (Vector2Int tile in affectedTiles) EFM.ToggleMineInTile(tile); };
                    else InverseOperation = () => { foreach (Vector2Int tile in affectedTiles) EFM.SetMineInTile(tile, AddedOrCleared); };
                    break;
                case editorTools.flag:
                    if (ToggledOrSet) InverseOperation = () => { foreach (Vector2Int tile in affectedTiles) EFM.ToggleFlagInTile(tile); };
                    else InverseOperation = () => { foreach (Vector2Int tile in affectedTiles) EFM.SetFlagInTile(tile, AddedOrCleared); };
                    break;
                case editorTools.reveal:
                    if (ToggledOrSet) InverseOperation = () => { foreach (Vector2Int tile in affectedTiles) EFM.ToggleCoverInTile(tile); };
                    else InverseOperation = () => { foreach (Vector2Int tile in affectedTiles) EFM.SetCoverInTile(tile, AddedOrCleared); };
                    break;
                case editorTools.create:
                    InverseOperation = () => { foreach (Vector2Int tile in affectedTiles) EFM.DeleteTile(tile); };
                    break;
                case editorTools.destroy:
                    InverseOperation = () =>
                    {
                        for (int i = 0; i < affectedTiles.Count; i++)
                        {
                            NewEditorTile.basicTileData btd = tileDatas[i];
                            EFM.CreateTile(affectedTiles[i], btd.hasMine, btd.hasFlag, btd.hasCover, btd.nearbyMines);
                        }
                    };
                    break;
            }
            affectedTiles = AffectedTiles;
        }
    }
}
