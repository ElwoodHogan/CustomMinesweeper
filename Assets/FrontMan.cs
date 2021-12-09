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

public class FrontMan : MonoBehaviour
{
    [SerializeField] int height;
    [SerializeField] int width;
    [SerializeField] int mines;
    [SerializeField] Tile tile;
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

    private void Awake()
    {
        FM = this;
    }
    private void Start()
    {
        
        //SetBoard();
        
    }

    [Button]
    void SetBoard()
    {
        SetBoard(height, width, mines);
    }
    public void SetBoard(int _height, int _width, int _mines)
    {
        playing = true;
        InGameMenuAI.IGM.Init(_height, _width, _mines);
        TotalTiles = _height * _width;
        mines = _mines;
        TotalFlagged = 0;
        TilesRevealed = 0;
        tiles = new List<Tile>(); 
        foreach (Transform child in TileParent) Destroy(child.gameObject);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                tiles.Add(Instantiate(tile, new Vector2(x, y), Quaternion.identity, TileParent));  //Spawning grid
                tiles[tiles.Count - 1].name = $"Tile x:{x}, y:{y}";
            }
        }

        
        Camera.main.transform.position = new Vector3(_height / 2, _width / 2, -10);
        Camera.main.orthographicSize = Mathf.Clamp((Mathf.Max(_height, _width) / 2)+3, 2, 20);  //Scales the camera to envolope larger maps
    }
    private void Update()
    {
        if(TilesRevealed == 0 && Input.GetMouseButtonDown(0))
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





        OnUpdate?.Invoke();
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
