using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static FrontMan;

public class FrontMan : MonoBehaviour
{
    public int height;
    public int width;
    [SerializeField] int mines;
    public NewTile OST;
    [SerializeField] Transform backGround;
    public HashSet<Vector2Int> mineLocations;
    List<Vector2Int> borderTiles;

    public Transform TileParent;
    public static FrontMan FM;
    public int TilesRevealed = 0;
    public int TotalFlagged = 0;
    public int TotalTiles = 0;
    public List<Color> NumberColors = new List<Color>();

    public bool playing = false;  //Dictates weather the player is ingame or not

    public System.Action OnUpdate;

    [SerializeField] GameObject mine;
    public List<Vector2Int> flags;
    public Dictionary<Vector2, GameObject> flagsDict;
    [SerializeField] GameObject flag;

    public int extraLives = 0;

    public SpriteRenderer grid;

    [SerializeField] Image LoadBar;
    [SerializeField] bool loadingMines = false;
    [SerializeField] GameObject heart;

    private void Awake()
    {
        FM = this;
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

        //Resetting OST stuff and setting a boarder around the board.  This saves the OST from having to do border checks itself
        NewTile.TilePosits = new HashSet<Vector2Int>();
        NewTile.NewTiles = new Dictionary<Vector2Int, NewTile>();

        borderTiles = new List<Vector2Int>();
        for (int i = -2; i <= width + 1; i++)
        {
            NewTile.TilePosits.Add(new Vector2Int(i, height));  //Top boarder
            NewTile.TilePosits.Add(new Vector2Int(i, height + 1));  //Top boarder width 2
            NewTile.TilePosits.Add(new Vector2Int(i, -1));      //bottom boarder
            NewTile.TilePosits.Add(new Vector2Int(i, -2));      //bottom boarder width 2
            borderTiles.Add(new Vector2Int(i, height));
            borderTiles.Add(new Vector2Int(i, -1));
        }
        for (int i = -2; i <= height + 1; i++)
        {
            NewTile.TilePosits.Add(new Vector2Int(-1, i));      //right boarder
            NewTile.TilePosits.Add(new Vector2Int(-2, i));      //right boarder width 2
            NewTile.TilePosits.Add(new Vector2Int(width, i));  //left boarder
            NewTile.TilePosits.Add(new Vector2Int(width + 1, i));  //left boarder width 2
            borderTiles.Add(new Vector2Int(-1, i));
            borderTiles.Add(new Vector2Int(width, i));
        }

        flags = new List<Vector2Int>();
        flagsDict = new Dictionary<Vector2, GameObject>();
        TotalTiles = height * width;
        TotalFlagged = 0;
        TilesRevealed = 0;
        TileParent.gameObject.SetActive(false);
        TileParent = new GameObject("NewTileParent").transform;
        TileParent.gameObject.SetActive(true);

        Transform background = Instantiate(backGround, TileParent).transform;
        background.localScale = new Vector3(width, height, 0);
        background.position = new Vector3(((float)width / 2), ((float)height / 2), 0);

        Camera.main.transform.position = new Vector3((float)width / 2, (float)height / 2, -10);
        Camera.main.orthographicSize = Mathf.Clamp((Mathf.Max(height, width) / 2) + 3, 2, SettingMenuAI.SM.GetCC() ? 20 : 999);  //Scales the camera to envolope larger maps
        grid.enabled = true;

        InGameMenuAI.IGM.Init(height, width, mines);
        playing = true;
    }

    private void Update()
    {
        OnUpdate?.Invoke();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int MWPFloored = mouseWorldPos.Floor();
        Vector2Int MWPFloored2 = MWPFloored.V3toV2Int();


        if (Input.GetKeyDown(KeyCode.Q) && TilesRevealed > 0 && playing)
        {
            Vector3 loc = NewTile.TilePosits.Where(
                v => flags.Where(
                    f => Vector2.Distance(v, f) < 2).Count()
                    != mineLocations.Where(l => Vector2.Distance(v, l) < 2).Count()
                    && !borderTiles.Contains(v)).
                    OrderBy(
                        T => Vector3.Distance(Camera.main.transform.position, (Vector2)T)).
                            ToList()[0].V2toV3(-10).
                                Change(.5f,.5f, 0);
            Camera.main.transform.DOMove(loc, 1);
        }
        if (Input.GetKeyDown(KeyCode.E) && TilesRevealed > 0 && playing)
        {
            Vector3 loc = NewTile.TilePosits.Where(
                v => flags.Where(
                    f => Vector2.Distance(v, f) < 2).Count()
                    != mineLocations.Where(l => Vector2.Distance(v, l) < 2).Count()
                    && !borderTiles.Contains(v)).ToList().
                        RandomPicker().V2toV3(-10).
                            Change(.5f, .5f, 0);
            Camera.main.transform.DOMove(loc, 1);
        }

        if (Input.GetKeyDown(KeyCode.G)) grid.enabled = !grid.enabled;


        if (!playing
            || loadingMines
            || EventSystem.current.IsPointerOverGameObject()
            || !Application.isFocused
            || !mouseWorldPos.x.FInRange(0, width)
            || !mouseWorldPos.y.FInRange(0, height)) return;

        

        if (TilesRevealed > 0
            && Input.GetMouseButtonDown(0))             //LEFT CLICK POST-INITAL REVEAL
        {
            if (flags.Contains(MWPFloored2)) return;  //If theres a flag, ignore the reveal

            if (mineLocations.Contains(MWPFloored2))
            {
                SpawnMine(MWPFloored2);
            }
            else if (NewTile.TilePosits.Contains(MWPFloored2))//IF CLICKING A REVEALED TILE, INITIATE A QUICK REVEAL
            {  
                NewTile.NewTiles[MWPFloored2].QuickReveal();
            }
            else
            {
                SpawnTile(MWPFloored2);
            }


        }

        if (TilesRevealed > 0 && Input.GetMouseButtonDown(1))  //RIGHT CLICK POST-INITIAL REVEAL
        {

            if (flags.Contains(MWPFloored2))    //If the player is right clicking a flag, remove it
            {
                Destroy(flagsDict[MWPFloored2]);
                flags.Remove(MWPFloored2);
                flagsDict.Remove(MWPFloored2);
                TotalFlagged--;
                return;
            }
            else if (NewTile.TilePosits.Contains(MWPFloored2))  //ELSE IF, the player has clicked a revealed tile, initiat a quick-flag
            {
                NewTile.NewTiles[MWPFloored2].QuickFlag();
            }
            else           //ELSE the player has clicked an unrevealed tile, in which,  a flag is created
            {
                CreateFlag(MWPFloored2);
            }
        }



        StartCoroutine(InitialClick(MWPFloored2));

        if ((TotalTiles - mines) == TilesRevealed && TotalFlagged == mines && playing)
        {
            InGameMenuAI.IGM.StopTimer();
            print("WINNER!!!");
            playing = false;
        }
    }

    IEnumerator InitialClick(Vector2Int MWPFloored2)
    {
        if (TilesRevealed == 0 && Input.GetMouseButtonDown(0) && !loadingMines)  //INITIAL CLICK: GENERATES MINE LOCATIONS
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Reset(); stopwatch.Start();
            loadingMines = true;
            LoadBar.gameObject.SetActive(true);
            mineLocations = new HashSet<Vector2Int>();

            float loaded = 0;
            float segment = 1;
            HashSet<Vector2Int> squareAroundFirstClick = new HashSet<Vector2Int>();
            for (int x = MWPFloored2.x - 1; x <= MWPFloored2.x + 1; x++)
                for (int y = MWPFloored2.y - 1; y <= MWPFloored2.y + 1; y++)
                    squareAroundFirstClick.Add(new Vector2Int(x, y));
            yield return 0;
            if (mines <= (TotalTiles * .995) + 1)  //HASHSET METHOD IS SLOWER FOR EXTREMELY SATURATED MAPS
            {
                print("hashset method");
                yield return 0;
                Vector2Int gridSize = new Vector2Int(width, height);
                if (NewGameMenuAI.NGM.SafetyMode.isOn)
                {
                    while (mineLocations.Count < mines)
                    {
                        Vector2Int vec = new Vector2Int(UnityEngine.Random.Range(1, gridSize.x), UnityEngine.Random.Range(1, gridSize.y));
                        if (!squareAroundFirstClick.Contains(vec)) mineLocations.Add(vec);

                        if ((((mineLocations.Count) / mines) * 100) > loaded)
                        {
                            loaded += segment;
                            LoadBar.fillAmount = (mines - mineLocations.Count) / mines;
                            yield return 0;
                        }
                    }
                }
                else
                {
                    while (mineLocations.Count < mines)
                    {
                        mineLocations.Add(new Vector2Int(UnityEngine.Random.Range(1, gridSize.x), UnityEngine.Random.Range(1, gridSize.y)));
                        if (((mineLocations.Count) / mines) * 100 > loaded)
                        {
                            loaded += segment;
                            LoadBar.fillAmount = (mines - mineLocations.Count) / mines;
                            yield return null;
                        }
                    }
                }
            }
            else
            {
                yield return null;
                List<Vector2Int> possibleLocations = new List<Vector2Int>();
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        possibleLocations.Add(new Vector2Int(x, y));
                    }
                }
                if (NewGameMenuAI.NGM.SafetyMode.isOn) foreach (Vector2Int vector in squareAroundFirstClick) possibleLocations.Remove(vector);
                while (mineLocations.Count < mines)
                {
                    Vector2Int vect = possibleLocations.RandomPicker();
                    possibleLocations.Remove(vect);
                    mineLocations.Add(vect);
                    if (((mineLocations.Count) / (float)mines * 100) > loaded)
                    {
                        loaded += segment;
                        LoadBar.fillAmount = (mines - mineLocations.Count) / mines;
                        yield return 0;
                    }
                }
            }
            print(mines);
            print(mineLocations.Count);
            LoadBar.gameObject.SetActive(false);


            if (mineLocations.Contains(MWPFloored2))
            {
                SpawnMine(MWPFloored2);
            }
            else
            {
                SpawnTile(MWPFloored2);
            }
            InGameMenuAI.IGM.StartTimer();
            loadingMines = false;
            print("Loading time: " + stopwatch.Elapsed);
            stopwatch.Stop();

        }
        yield return 0;
    }
    public void DeActivateBoard(int recursion)
    {
        int limit = recursion + 50;
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
            try { Destroy(TileParent.GetChild(i).gameObject); } catch (Exception) { }
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

    [Button]
    public void CopyToClipboardTest()
    {
        GUIUtility.systemCopyBuffer = "joe mama";
    }
    [Button]
    public void PrintToClipboardTest()
    {
        print(GUIUtility.systemCopyBuffer);
    }

    public void CreateFlag(Vector2Int pos)
    {
        flags.Add(pos);
        GameObject fl = Instantiate(flag, pos.V2IntToV3(), Quaternion.identity, TileParent);
        flagsDict.Add(pos, fl);
        TotalFlagged++;
    }

    void SpawnMine(Vector2Int pos)
    {
        Instantiate(mine, pos.V2IntToV3(), Quaternion.identity, TileParent);
        extraLives--;
        if (extraLives < 0)
        {
            InGameMenuAI.IGM.StopTimer();
            print("YOU LOSE");
        }
        else
        {
            flags.Add(pos);
            Destroy(Instantiate(heart, pos.Change(.5f,.5f), Quaternion.identity), 1);
            TotalFlagged++;
        }
    }

    void SpawnTile(Vector2Int loc)
    {
        if (NewTile.TilePosits.Add(loc))
        {
            NewTile nt = Instantiate(OST, loc.V2IntToV3(), Quaternion.identity, TileParent);
            NewTile.NewTiles.Add(loc, nt);
        }
    }

    [Button]
    public void SortTiles()
    {
        List<NewTile> tempTiles = new Dictionary<Vector2Int, NewTile>(NewTile.NewTiles).Values.ToList().OrderBy(nt=>nt.pos.x).ThenBy(t=>t.pos.y).ToList();
        for (int i = 0; i < tempTiles.Count; i++)
        {
            tempTiles[i].transform.SetSiblingIndex(i);
        }
    }
}

public class ExtendedMonoBehaviour : MonoBehaviour
{
}

public static class Helper
{
    public static Vector3 Change(this Vector3 pos, float x, float y, float z)
    {
        return pos + new Vector3(x, y, z);
    }
    public static Vector2 Change(this Vector2 pos, float x, float y)
    {
        return pos + new Vector2(x, y);
    }
    public static Vector2 Change(this Vector2Int pos, float x, float y)
    {
        return pos + new Vector2(x, y);
    }

    public static T RandomPicker<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static int ToInt(this bool b)
    {
        return b ? 1 : 0;
    }

    public static Vector3Int Floor(this Vector3 v)
    {
        return new Vector3Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), 0);
    }

    public static bool FInRange(this float x, float incBottom, float incTop)
    {
        return (x >= incBottom && x <= incTop);
    }

    public static bool IInRange(this int x, float incBottom, float incTop)
    {
        return (x >= incBottom && x <= incTop);
    }

    public static Vector2 V3toV2(this Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }

    public static Vector3 V2toV3(this Vector2Int v2, float z)
    {
        return new Vector3(v2.x, v2.y, z);
    }

    public static Vector2Int V3toV2Int(this Vector3Int v3)
    {
        return new Vector2Int(v3.x, v3.y);
    }

    public static Vector3 V2toV3(this Vector2 v2, float z)
    {
        return new Vector3(v2.x, v2.y, z);
    }

    public static Vector3 V2IntToV3(this Vector2Int v2)
    {
        return new Vector3(v2.x, v2.y, 0);
    }

    public static Vector2 LerpByDistance(Vector2 A, Vector2 B, float x)
    {
        Vector2 P = x * (B - A).normalized + A;
        return P;
    }

    public static Vector2 Absolute(this Vector2 vect)
    {
        return new Vector2(Mathf.Abs(vect.x), Mathf.Abs(vect.y));
    }

    public static Vector2Int Floor(this Vector2 vect)
    {
        return new Vector2Int(Mathf.FloorToInt(vect.x), Mathf.FloorToInt(vect.y));
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