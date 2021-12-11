using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine.EventSystems;
using static FrontMan;
using DG.Tweening;

public class FrontMan : MonoBehaviour
{
    public int height;
    public int width;
    [SerializeField] int mines;
    public Tile tile;
    [SerializeField] Transform backGround;
    public List<Vector2> mineLocations;
    public Transform TileParent;
    public List<Tile> tiles;
    public static FrontMan FM;
    public int TilesRevealed = 0;
    public int TotalFlagged = 0;
    public int TotalTiles = 0;
    public GameObject circleShower;
    public List<Color> NumberColors = new List<Color>();

    public bool playing = false;  //Dictates weather the player is ingame or not

    public System.Action OnUpdate;

    public int TileRevealsPerFrame;

    int backgroundSpawns = 10;
    public Transform bsTrans;
    public int bsTransCC = 0;

    private void Awake()
    {
        FM = this;
    }
    private void Start()
    {

        //SetBoard();
        bsTrans = new GameObject("bsTrans").transform;
        StartCoroutine(spawner());
    }

    IEnumerator spawner()
    {
        yield return 0;
        if (!playing)
        {
            for (int i = 0; i < backgroundSpawns; i++)
            {
                Instantiate(tile, new Vector3(-99, -99, 0), Quaternion.identity, bsTrans);
            }
        }
        StartCoroutine(spawner());
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
        playing = true;
        tiles = new List<Tile>();
        InGameMenuAI.IGM.Init(height, width, mines);
        TotalTiles = height * width;
        TotalFlagged = 0;
        TilesRevealed = 0;
        TileParent.gameObject.SetActive(false);
        TileParent = new GameObject("NewTileParent").transform;
        TileParent.gameObject.SetActive(true);

        Transform background =  Instantiate(backGround, TileParent).transform;
        background.localScale = new Vector3(width, height, 0);
        background.position = new Vector3((width / 2), (height / 2), 0);

        Camera.main.transform.position = new Vector3(width / 2, height / 2, -10);
        Camera.main.orthographicSize = Mathf.Clamp((Mathf.Max(height, width) / 2) + 3, 2, SettingMenuAI.SM.GetCC() ? 20 : 999);  //Scales the camera to envolope larger maps
    }
    public void OLDSetBoard(int _height, int _width, int _mines)
    {
        height = _height;
        width = _width;
        mines = _mines;
        playing = true;
        InGameMenuAI.IGM.Init(height, width, mines);
        TotalTiles = height * width;
        TotalFlagged = 0;
        TilesRevealed = 0;
        tiles = new List<Tile>();
        foreach (Transform child in TileParent) Destroy(child.gameObject);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles.Add(Instantiate(tile, new Vector2(x, y), Quaternion.identity, TileParent));  //Spawning grid
                tiles[tiles.Count - 1].name = $"Tile x:{x}, y:{y}";
            }
        }


        Camera.main.transform.position = new Vector3(width / 2, height / 2, - 10);
        Camera.main.orthographicSize = Mathf.Clamp((Mathf.Max(height, width) / 2) + 3, 2, SettingMenuAI.SM.GetCC() ? 20 : 999);  //Scales the camera to envolope larger maps
    }

    private void Update()
    {
        bsTransCC = bsTrans.childCount;
        OnUpdate?.Invoke();
        TileRevealsPerFrame = Tile.totalRevealsThisFrame;
        Tile.totalRevealsThisFrame = 0;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!playing 
            || !mouseWorldPos.x.FInRange(0, width)
            || !mouseWorldPos.y.FInRange(0, height)) return;
        

        if (TilesRevealed > 0 
            && Input.GetMouseButtonDown(0))
        {
            Tile t = null;
            try
            {
                t = Physics2D.Raycast(mouseWorldPos, Vector2.zero).transform.GetComponent<Tile>();
            }
            catch (NullReferenceException)
            {
                t = Instantiate(tile, mouseWorldPos.Floor().Change(0, 0, 10), Quaternion.identity, TileParent);
                t.transform.SetParent(TileParent);
                tiles.Add(t);
                t.Init(gameObject);
                t.Reveal();
            }

            
        }

        if (TilesRevealed > 0 && Input.GetMouseButtonDown(1))
        {
            Tile t = null;
            try
            {
                t = Physics2D.Raycast(mouseWorldPos, Vector2.zero).transform.GetComponent<Tile>();
            }
            catch (NullReferenceException)
            {
                print(mouseWorldPos.Floor());
                t = Instantiate(tile, mouseWorldPos.Floor().Change(0, 0, 10), Quaternion.identity, TileParent);
                t.transform.SetParent(TileParent);
                tiles.Add(t);
                t.Init(gameObject);
                t.ToggleFlag();
            }
        }


        if (TilesRevealed == 0 && Input.GetMouseButtonDown(0))
        {
            Tile ClickedTile = Instantiate(tile, mouseWorldPos.Floor().Change(0,0,10), Quaternion.identity, TileParent);
            tiles.Add(ClickedTile);
            int tempMines = mines;
            mineLocations = new List<Vector2>();
            print(ClickedTile.pos);
            while (tempMines > 0)
            {
                int randX;
                int randY;
                do  {
                    randX = UnityEngine.Random.Range(0, width - 1);
                    randY = UnityEngine.Random.Range(0, height - 1);
                } while ((randX.IInRange(ClickedTile.pos.x - 1, ClickedTile.pos.x + 1) 
                      && randY.IInRange(ClickedTile.pos.y - 1, ClickedTile.pos.y + 1))
                      || mineLocations.Contains(new Vector2(randX, randY)));

                mineLocations.Add(new Vector2(randX, randY));
                tempMines--;
            }
            mineLocations = mineLocations.OrderBy(t => t.x).ThenBy(t => t.y).ToList();
            ClickedTile.Init(gameObject);
            ClickedTile.Reveal();
            InGameMenuAI.IGM.StartTimer();
        }

        

        if ((TotalTiles - mines) == TilesRevealed && TotalFlagged == mines && playing)
        {
            InGameMenuAI.IGM.StopTimer();
            print("WINNER!!!");
            playing = false;
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            Camera.main.transform.DOMove(tiles.Where(T => !T.Flagged && !T.revealed).OrderBy(T => Vector3.Distance(Camera.main.transform.position, T.pos)).ToList()[0].pos.Change(0, 0, -10), 1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Camera.main.transform.DOMove(tiles.Where(T => T.revealed && FM.tiles.Where(tile => T.NearbyTilePos.Contains(tile.pos2) && !tile.Flagged).ToList().Count > 0).ToList().RandomPicker<Tile>().pos.Change(0, 0, -10), 1);
        }



        
    }
    private void OLDUpdate()
    {
        if (TilesRevealed == 0 && Input.GetMouseButtonDown(0))
        {
            var hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.forward, 100f);  //Getting the tile that was clicked
            Tile ClickedTile = null;
            foreach (var hit in hits)
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile) ClickedTile = tile;
            }
            if (ClickedTile == null)
            {
                return;
            }
            List<Tile> tempList = new List<Tile>(tiles);
            if (NewGameMenuAI.NGM.SafetyMode.isOn)
            {
                var surrounding = Physics2D.OverlapCircleAll(ClickedTile.pos, 1);
                foreach (var collider in surrounding)
                {
                    tempList.Remove(collider.GetComponent<Tile>());
                }
            }
            int tempMines = mines;
            while (tempMines > 0)
            {
                int i = UnityEngine.Random.Range(0, tempList.Count);
                tempList[i].ContainsMine = true;
                tempList.RemoveAt(i);
                tempMines--;
            }
            ClickedTile.Reveal();
            InGameMenuAI.IGM.StartTimer();
        }

        if ((TotalTiles - mines) == TilesRevealed && TotalFlagged == mines && playing)
        {
            InGameMenuAI.IGM.StopTimer();
            print("WINNER!!!");
            playing = false;
        }


        if(Input.GetKeyDown(KeyCode.Q) && playing)
        {
            Camera.main.transform.DOMove(tiles.Where(T=>!T.Flagged&&!T.revealed).OrderBy(T=>Vector3.Distance(Camera.main.transform.position, T.pos)).ToList()[0].pos.Change(0,0,-10), 1);
        }
        if (Input.GetKeyDown(KeyCode.E) && playing)
        {
            //Camera.main.transform.DOMove(tiles.Where(T => T.revealed && T.NearbyTiles.Where(tile=>!tile.Flagged && !tile.revealed).ToList().Count > 0).ToList().RandomPicker<Tile>().pos.Change(0, 0, -10), 1);
        }



        OnUpdate?.Invoke();
    }

    [Button]
    public void OrderTiles()
    {
        tiles = tiles.OrderBy(t => t.pos.x).ThenBy(t => t.pos.y).ToList();
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].transform.SetSiblingIndex(i);
        }
    }
    public void DestroyTileParent()
    {
        Destroy(TileParent.gameObject);
        GameObject TileParentTemp = new GameObject();
        TileParent = TileParentTemp.transform;
    }
    public void DeActivateBoard(int recursion)
    {
        int limit = recursion+50;
        for (; recursion < limit; recursion++)
        {
            try { TileParent.GetChild(recursion).gameObject.SetActive(false); } catch (Exception) { }
            if (TileParent.childCount == recursion) return;
        }
        StartCoroutine(WaitToDeActivate(recursion));
    }
    IEnumerator WaitToDeActivate(int recursion)
    {
        yield return 0;
        DeActivateBoard(recursion);
    }

    public void DestroyBoard()
    {
        for (int i = 0; i < 50; i++)
        {
            try { Destroy(TileParent.GetChild(i).gameObject); } catch (Exception) {}
            if (TileParent.childCount == 0) return;
        }
        StartCoroutine(WaitToDestroy());
    }

    IEnumerator WaitToDestroy()
    {
        yield return 0;
        DestroyBoard();
    }

    public void EXitGame()
    {
        Application.Quit();
    }
}

public class ExtendedMonoBehaviour: MonoBehaviour
{
}

public static class Helper
{
    public static Vector3 Change(this Vector3 pos, float x, float y, float z)
    {
        return pos + new Vector3(x, y, z);
    }

    public static T RandomPicker<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static int ToInt(this bool b)
    {
        return b ? 1 : 0;
    }

    public static Vector3 Floor(this Vector3 v)
    {
        return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), 0);
    }

    public static bool FInRange(this float x, float incBottom, float incTop)
    {
        return (x >= incBottom && x <= incTop) ;
    }

    public static bool IInRange(this int x, float incBottom, float incTop)
    {
        return (x >= incBottom && x <= incTop);
    }

    public static Vector2 V3toV2(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }
}

public class Timer
{
    public Action job;
    private task Task;
    public Func<float> TimerThreshhold;
    public float TimerThreshholdFloat;
    public float timer = 0;
    public string JobName = "";  //For debugging purposes, i can attach names to jobs which will be stored here
    GameObject AttachedObject;  //Should this object be destroyed, the repeat invoked will discontinue
    /*  [BAD PRACTICE]  If you need a constant repeater, attach this to the GM.
    public static Timer RepeatInvoker(Action Job, Func<float> repeatTime)  //This repeat invoker will repeat indefinitley
    {
        Timer timer = new Timer();
        timer.TimerThreshhold = repeatTime;
        timer.job = Job;
        timer.Task = task.repeatInvoker;
        return timer;
    }*/

    //Set float to 0 for an occurence every frame
    public static Timer RepeatInvoker(Action Job, Func<float> repeatTime, GameObject AttachTo)//This repeat invoker will repeat as long as the attached gameobject is NOT destroyed
    {
        Timer timer = new Timer();
        timer.TimerThreshhold = repeatTime;
        timer.job = Job;
        timer.Task = task.repeatInvoker;
        timer.AttachedObject = AttachTo;
        return timer;
    }
    public static Timer RepeatInvoker(Action Job, Func<float> repeatTime, GameObject AttachTo, string TaskTitle)//This repeat invoker will repeat as long as the attached gameobject is NOT destroyed
    {
        Timer timer = new Timer();
        timer.TimerThreshhold = repeatTime;
        timer.job = Job;
        timer.Task = task.repeatInvoker;
        timer.AttachedObject = AttachTo;
        timer.JobName = TaskTitle;
        return timer;
    }

    public static Timer RepeatInvoker(Action Job, float repeatTime, GameObject AttachTo)//overload for regular float input
    {
        return RepeatInvoker(Job, () => repeatTime, AttachTo);
    }
    public static Timer SimpleTimer(Action Job, float timeSeconds)
    {
        Timer timer = new Timer();
        timer.TimerThreshholdFloat = timeSeconds;
        timer.job = Job;
        timer.Task = task.simpleTimer;
        return timer;
    }

    public Timer()
    {
        FM.OnUpdate += Update;
    }

    ~Timer()
    {
        FM.OnUpdate -= Update;
    }

    public void Update()
    {
        switch (Task)
        {
            case task.repeatInvoker:
                if (timer < TimerThreshhold())
                {
                    timer += Time.deltaTime;
                    //ConsoleProDebug.Watch("Repeat Timer for "+ JobName + ": ", "" + timer);

                }
                else
                {
                    job?.Invoke();
                    if (AttachedObject)
                    {
                        timer = 0;
                    }
                    else
                    {
                        job = null;
                        FM.OnUpdate -= Update;
                    }
                }
                break;
            case task.simpleTimer:
                if (timer < TimerThreshholdFloat)
                {
                    timer += Time.deltaTime;
                    //ConsoleProDebug.Watch("Simple Timer: ", "" + timer);
                }
                else
                {
                    job?.Invoke();
                    FM.OnUpdate -= Update;
                }
                break;
        }
    }

    private enum task
    {
        repeatInvoker,
        simpleTimer,
    }
}