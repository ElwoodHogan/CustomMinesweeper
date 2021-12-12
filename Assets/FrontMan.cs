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
    public OnScreenTester OST;
    [SerializeField] Transform backGround;
    public List<Vector2> mineLocations;

    public Transform TileParent;
    public List<Tile> tiles;
    public static FrontMan FM;
    public int TilesRevealed = 0;
    public int TotalFlagged = 0;
    public int TotalTiles = 0;
    public List<Color> NumberColors = new List<Color>();

    public bool playing = false;  //Dictates weather the player is ingame or not

    public System.Action OnUpdate;

    [SerializeField] GameObject mine;
    List<Vector2> flags;
    [SerializeField] GameObject flag;

    private void Awake()
    {
        FM = this;
    }
    private void Start()
    {

        //SetBoard();
        //bsTrans = new GameObject("bsTrans").transform;
        //StartCoroutine(spawner());
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
        OnScreenTester.TilePosits = new HashSet<Vector2>();
        for (int i = -1; i <= width; i++)
        {
            OnScreenTester.TilePosits.Add(new Vector2(i, height));  //Top boarder
            OnScreenTester.TilePosits.Add(new Vector2(i, -1));      //bottom boarder
        }
        for (int i = -1; i <= height; i++)
        {
            OnScreenTester.TilePosits.Add(new Vector2(-1, i));      //right boarder
            OnScreenTester.TilePosits.Add(new Vector2(width, i));  //left boarder
        }





        tiles = new List<Tile>();
        flags = new List<Vector2>();
        TotalTiles = height * width;
        TotalFlagged = 0;
        TilesRevealed = 0;
        TileParent.gameObject.SetActive(false);
        TileParent = new GameObject("NewTileParent").transform;
        TileParent.gameObject.SetActive(true);

        Transform background =  Instantiate(backGround, TileParent).transform;
        background.localScale = new Vector3(width, height, 0);
        background.position = new Vector3(((float)width / 2), ((float)height / 2), 0);

        Camera.main.transform.position = new Vector3((float)width / 2, (float)height / 2, -10);
        Camera.main.orthographicSize = Mathf.Clamp((Mathf.Max(height, width) / 2) + 3, 2, SettingMenuAI.SM.GetCC() ? 20 : 999);  //Scales the camera to envolope larger maps

        InGameMenuAI.IGM.Init(height, width, mines);
        playing = true;
    }

    private void Update()
    {
        //bsTransCC = bsTrans.childCount;
        OnUpdate?.Invoke();
        Tile.totalRevealsThisFrame = 0;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 MWPFloored = mouseWorldPos.Floor();
        Vector2 MWPFloored2 = MWPFloored.V3toV2();
        ConsoleProDebug.Watch("MWPFloored2", MWPFloored2 + "");
        if (!playing 
            || !mouseWorldPos.x.FInRange(0, width)
            || !mouseWorldPos.y.FInRange(0, height)) return;
        if(mineLocations.Count > 0) ConsoleProDebug.Watch("Mouse is over mine", mineLocations.Contains(MWPFloored2) + "");

        if (TilesRevealed > 0 
            && Input.GetMouseButtonDown(0))             //LEFT CLICK POST-INITAL REVEAL
        {
            if (flags.Contains(MWPFloored2)) return;  //If theres a flag, ignore the reveal

            if (mineLocations.Contains(MWPFloored2))
            {
                Instantiate(mine, MWPFloored2, Quaternion.identity);
                //lose
            }else if(OnScreenTester.TilePosits.Contains(MWPFloored2)){  //IF CLICKING A REVEALED TILE, INITIATE A QUICK REVEAL
                List<Collider2D> nearby = Physics2D.OverlapCircleAll(MWPFloored2.Change(.5f, .5f), 1).ToList();
                //nearby.Remove(Physics2D.Raycast(MWPFloored2.Change(.5f, .5f), Vector3.forward).collider);
                //print(nearby.Length);
                int nearbyFlags = nearby.Where(c => c.gameObject.GetComponent<OnScreenTester>() == null).Count();
                //print(nearbyFlags);
                //int nearbyHiddenUnflagged = 8 - (nearby.Length - 1);
                //print(nearbyHiddenUnflagged);
                if (nearbyFlags == FM.mineLocations.Where(v => Vector2.Distance(v, MWPFloored2) < 2).Count())
                {
                        List<Vector2> possibleReveals = new List<Vector2>();
                        for (int x = (int)MWPFloored2.x - 1; x <= MWPFloored2.x + 1; x++)
                        {
                            for (int y = (int)MWPFloored2.y - 1; y <= MWPFloored2.y + 1; y++)
                            {
                                possibleReveals.Add(new Vector2(x, y));
                            }
                        }
                    possibleReveals.Remove(MWPFloored2);
                        print(possibleReveals.Where(v => nearby.Where(c => c.transform.position.V3toV2() == v).Count() == 0).Count());
                        possibleReveals = possibleReveals.Where(v => nearby.Where(c => c.transform.position.V3toV2() == v).Count() == 0).ToList();
                        print(possibleReveals);
                        foreach (var vector in possibleReveals) Instantiate(OST, vector, Quaternion.identity, TileParent);
                }
            }
            else
            {
                Instantiate(OST, MWPFloored2, Quaternion.identity, TileParent);
            }

            
        }

        if (TilesRevealed > 0 && Input.GetMouseButtonDown(1))  //RIGHT CLICK POST-INITIAL REVEAL
        {
            var hit = Physics2D.Raycast(MWPFloored2.Change(.5f,.5f), Vector3.forward);
            
            if (hit)
            {
                print(hit.transform.gameObject.name);
                print(hit);
                if (flags.Contains(MWPFloored2))
                {
                    Destroy(hit.transform.gameObject);
                    flags.Remove(MWPFloored2);
                    return;
                }
                else
                {
                    Collider2D[] nearby = Physics2D.OverlapCircleAll(MWPFloored2.Change(.5f, .5f), 1);
                    print(nearby.Length);
                    int nearbyFlags = nearby.Where(c => c.gameObject.GetComponent<OnScreenTester>() == null).Count();
                    print(nearbyFlags);
                    int nearbyHiddenUnflagged = 8 - (nearby.Length - 1);
                    print(nearbyHiddenUnflagged);
                    if (nearbyHiddenUnflagged != 0)
                    {
                        if (FM.mineLocations.Where(v => Vector2.Distance(v, MWPFloored2) < 2).Count() == nearbyHiddenUnflagged)
                        {
                            List<Vector2> possibleReveals = new List<Vector2>();
                            for (int x = (int)MWPFloored2.x - 1; x <= MWPFloored2.x + 1; x++)
                            {
                                for (int y = (int)MWPFloored2.y - 1; y <= MWPFloored2.y + 1; y++)
                                {
                                    possibleReveals.Add(new Vector2(x, y));
                                }
                            }
                            print(possibleReveals.Where(v => nearby.Where(c => c.transform.position.V3toV2() == v).Count() == 0).Count());
                            possibleReveals = possibleReveals.Where(v => nearby.Where(c => c.transform.position.V3toV2() == v).Count() == 0).ToList();
                            print(possibleReveals);
                            foreach (var vector in possibleReveals) Instantiate(OST, vector, Quaternion.identity, TileParent);
                        }
                    }
                }
            }
            else
            {
                flags.Add(MWPFloored2);
                Instantiate(flag, MWPFloored2, Quaternion.identity, TileParent);
            }
        }


        if (TilesRevealed == 0 && Input.GetMouseButtonDown(0))
        {
            
            mineLocations = new List<Vector2>();
            //print(ClickedTile.pos);
            //int randX;
            //int randY;
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Reset();
            //stopwatch.Start();
            List<Vector2> possibleMineLocations = new List<Vector2>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    possibleMineLocations.Add(new Vector2(x, y));
                }
            }
            if (NewGameMenuAI.NGM.SafetyMode.isOn) possibleMineLocations = new List<Vector2>(possibleMineLocations.Where(v => Vector2.Distance(v, MWPFloored2) >= 2).ToList());
            //stopwatch.Stop();
            //Debug.Log("Filling possible list:" +stopwatch.Elapsed);
            int tempMines = mines;
            Vector2 vect;
            while (tempMines > 0)
            {
                vect = possibleMineLocations[UnityEngine.Random.Range(0, possibleMineLocations.Count)];
                possibleMineLocations.Remove(vect);
                mineLocations.Add(vect);
                tempMines--;
            }
            OnScreenTester ClickedTile = Instantiate(OST, MWPFloored, Quaternion.identity, TileParent);
            InGameMenuAI.IGM.StartTimer();
        }

        if ((TotalTiles - mines) == TilesRevealed && TotalFlagged == mines && playing)
        {
            //InGameMenuAI.IGM.StopTimer();
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
    public static Vector2 Change(this Vector2 pos, float x, float y)
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