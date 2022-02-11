using UnityEngine;

public class Tile : MonoBehaviour
{
    /*
    [SerializeField] GameObject Cover;
    [SerializeField] GameObject Number;
    [SerializeField] GameObject Mine;
    [SerializeField] GameObject FlagP;
    [SerializeField] GameObject Flag = null;
    [SerializeField] GameObject Creator;
    [SerializeField] Renderer whiteSquar;
    public bool revealed = false;
    public bool Flagged = false;

    [SerializeField] bool _ContainsMine = false;
    public List<Vector2> NearbyTilePos = new List<Vector2>();
    public int NearbyMines = 0;

    public const int MAX_REVEALS_PER_FRAME = 30;
    public static int totalRevealsThisFrame = 0;

    public bool ContainsMine
    {
        get { return _ContainsMine; }
        set
        {
            _ContainsMine = value;
        }
    }
    public Vector3 pos => transform.position;
    public Vector2 pos2 => new Vector3(transform.position.x, transform.position.y);
    public Vector3 Lpos => transform.localPosition;

    public void Init(GameObject c)
    {
        name = $"x:{pos.x}, y:{pos.y}";
        NearbyTilePos = GetNearbyTilePos();
        Creator = c;
    }

    private void OnMouseOver()
    {
        if (FM.TilesRevealed == 0 || !FM.playing || InGameMenuAI.IGM.settingsOut) return;  //Returns if the board hasnt been generated yet, or the game is over
        if (Input.GetMouseButtonDown(0))
        {
            if (!revealed) Reveal();
            else CheckNearbyLeftClick();         //if the tile is already revealed, commit a quick reveal
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (!revealed) ToggleFlag();
            else CheckNearbyRightClick();       //if the tile is already revealed, commit a quick flag
        }
        //if (Input.GetMouseButtonDown(2)) Highlight();
    }

    public void Reveal()  //A recursion parameter is added as if a large amount of revealing happened at once it would cause a stack overflow
    {
        if (Flagged || revealed) return;  //Flagging a tiles prevents acciedentally revealing it
        revealed = true;
        ContainsMine = FM.mineLocations.Contains(pos2);
        if (!ContainsMine)
        {
            FM.TilesRevealed++;
            //print(FM.mineLocations.Count);
            NearbyMines = FM.mineLocations.Intersect(NearbyTilePos).Count();
            //print(name + ": " +NearbyMines);
            if (NearbyMines > 0)
            {
                Number = Instantiate(Number, transform);
                TextMeshPro tmp = Number.GetComponent<TextMeshPro>();
                tmp.text = NearbyMines + "";
                tmp.color = FM.NumberColors[NearbyMines];
            }
            else  //if the tile is not near a mine, reveal ALL surrounding un-revealed tiles
            {
                if (totalRevealsThisFrame < 0)
                {
                    //print(NearbyTilePos.Count);


                    foreach (Vector2 posit in NearbyTilePos.Where(v => !FM.tiles.Select(t => t.pos2).ToList().Contains(v)))
                    {
                        Tile newTile = Instantiate(FM.tile, posit, Quaternion.identity, FM.TileParent);
                        newTile.Init(gameObject);
                        FM.tiles.Add(newTile);
                        newTile.Reveal();
                    }
                    totalRevealsThisFrame++;
                }
                else StartCoroutine(WaitToReveal());
            }
        }
        else  //if it does contain a mine, dont worry about anything else, just initiate a lost
        {
            if (!FM.playing) return;
            Instantiate(Mine, transform);
            InGameMenuAI.IGM.StopTimer();
            print("LOSER!!!");
            FM.playing = false;
            Timer.SimpleTimer(() => { foreach (Tile tile in FM.tiles) tile.Reveal(); }, 2);
        }
    }
    [Button]
    public void getnearbymines()
    {
        print(NearbyMines = FM.mineLocations.Intersect(NearbyTilePos).Count());
    }
    IEnumerator WaitToReveal()
    {
        yield return 0;
        if (totalRevealsThisFrame < MAX_REVEALS_PER_FRAME)
        {
            //print(NearbyTilePos.Count);


            foreach (Vector2 posit in NearbyTilePos.Where(v => !FM.tiles.Select(t => t.pos2).ToList().Contains(v)))
            {
                Tile newTile = Instantiate(FM.tile, posit, Quaternion.identity, FM.TileParent);
                newTile.Init(gameObject);
                FM.tiles.Add(newTile);
                newTile.Reveal();
            }
            totalRevealsThisFrame++;
        }
        else StartCoroutine(WaitToReveal());
    }

    void CheckNearbyLeftClick()
    {
        List<Tile> tempTiles = new List<Tile>();
        foreach (Vector2 posit in NearbyTilePos)
        {
            Tile t = null;
            try
            {
                t = Physics2D.Raycast(new Vector2(posit.x + .5f, posit.y + .5f), Vector3.forward).transform.GetComponent<Tile>();
                tempTiles.Add(t);
            }
            catch (NullReferenceException)
            {
                t= Instantiate(FM.tile, posit, Quaternion.identity, FM.TileParent);
                t.transform.SetParent(FM.TileParent);
                FM.tiles.Add(t);
                t.Init(gameObject);
                tempTiles.Add(t);
            }
        }
        List<Tile> FlaggedTiles = tempTiles.Where(T => T.Flagged).ToList();
        List<Tile> UnFlaggedTiles = tempTiles.Where(T => !T.Flagged).ToList();
        if (FlaggedTiles.Count == NearbyMines) foreach (var tile in UnFlaggedTiles) tile.Reveal();
    }
    void CheckNearbyRightClick()
    {
        List<Tile> tempTiles = new List<Tile>();
        foreach(Vector2 posit in NearbyTilePos)
        {
            Tile t = null;
            try
            {
                t = Physics2D.Raycast(new Vector2(posit.x+.5f, posit.y+.5f), Vector3.forward).transform.GetComponent<Tile>();
                tempTiles.Add(t);
            }
            catch (NullReferenceException)
            {
                t = Instantiate(FM.tile, posit, Quaternion.identity, FM.TileParent);
                t.transform.SetParent(FM.TileParent);
                FM.tiles.Add(t);
                t.Init(gameObject);
                tempTiles.Add(t);
            }
        }
        List<Tile> UnrevealedTiles = tempTiles.Where(T => !T.Flagged && !T.revealed).ToList();
        print(UnrevealedTiles.Count);
        int FlaggedTiles = tempTiles.Where(T =>  T.Flagged).ToList().Count();
        if (UnrevealedTiles.Count + FlaggedTiles == NearbyMines) foreach (var tile in UnrevealedTiles) tile.ToggleFlag();
    }

    public void ToggleFlag()
    {
        if (revealed) return;
        Cover = Instantiate(Cover, transform);
        if (Flagged) FM.TotalFlagged--;
        else FM.TotalFlagged++;
        Flagged = !Flagged;

        if (Flag) Flag.SetActive(Flagged);
        else
        {
            Flag.SetActive(Flagged);
        }
    }
    public void SetFlag(bool state)
    {
        if (revealed) return;
        Flagged = !state;
        if (Flagged) FM.TotalFlagged--;
        else FM.TotalFlagged++;
        Flagged = !Flagged;
        //Flag.enabled = Flagged;
    }

    void Highlight()
    {
        //FM.circleShower.transform.position = pos;
    }

    public List<Vector2> GetNearbyTilePos()
    {
        List<Vector2> nearbyTiles = new List<Vector2>();
        if (pos.x - 1 >= 0)                                         nearbyTiles.Add(new Vector2(pos.x - 1, pos.y));
        if (pos.x + 1 <= FM.width-1)                                nearbyTiles.Add(new Vector2(pos.x + 1, pos.y));
        if (pos.y - 1 >= 0)                                         nearbyTiles.Add(new Vector2(pos.x, pos.y - 1));
        if (pos.y + 1 <= FM.height - 1)                             nearbyTiles.Add(new Vector2(pos.x, pos.y + 1));
        if (pos.x - 1 >= 0 && pos.y - 1 >= 0)                       nearbyTiles.Add(new Vector2(pos.x - 1, pos.y-1));
        if (pos.x + 1 <= FM.width -1 && pos.y + 1 <= FM.height-1)   nearbyTiles.Add(new Vector2(pos.x + 1, pos.y + 1));
        if (pos.x - 1 >= 0 && pos.y + 1 <= FM.height - 1)           nearbyTiles.Add(new Vector2(pos.x - 1, pos.y + 1));
        if (pos.x + 1 <= FM.width - 1 && pos.y - 1 >= 0)            nearbyTiles.Add(new Vector2(pos.x + 1, pos.y - 1));
        return nearbyTiles;
    }

    public List<Tile> GetNearbyTiles()
    {
        return FM.tiles.Where(tile => NearbyTilePos.Contains(tile.pos2)).ToList();
    }
    */
}