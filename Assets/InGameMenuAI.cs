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

public class InGameMenuAI : MonoBehaviour
{
    [SerializeField] Text Mines;
    [SerializeField] Text TimerText;
    [SerializeField] Text StartHeight;
    [SerializeField] Text Startwidth;
    [SerializeField] Text StartMines;
    [SerializeField] Text FlagAllText;
    [SerializeField] Text ExitText;
    [SerializeField] float timeStart = 0;
    [SerializeField] int height;
    [SerializeField] int width;
    [SerializeField] int startMines;
    public static InGameMenuAI IGM;
    private void Awake()
    {
        IGM = this;
    }

    private void Update()
    {
        if (timeStart != 0) TimerText.text = Mathf.Round(Time.time - timeStart) + "";
        if (FM.playing) Mines.text = startMines - FM.TotalFlagged+"";
    }

    public void Init(int _height, int _width, int _startMines)
    {
        StartHeight.text = _height+"";
        Startwidth.text = _width + "";
        StartMines.text = _startMines + "";
        startMines = _startMines;
    }

    public void StartTimer()
    {
        timeStart = Time.time;
    }
    public void StopTimer()
    {
        TimerText.text = Mathf.Round((Time.time - timeStart) * 100) / 100+"";
        timeStart = 0;
    }

    bool FlaggedAll = false;
    List<Tile> flaggedTiles = new List<Tile>();
    public void FlagAll()
    {
        if (!FlaggedAll)
        {
            flaggedTiles = FM.tiles.Where(t => !t.revealed&& !t.Flagged).ToList();
            foreach (Tile tile in flaggedTiles)
            {
                tile.ToggleFlag();
            }
            FlagAllText.text = "Undo?";
        }
        else
        {
            flaggedTiles = flaggedTiles.Where(t => !t.revealed && t.Flagged).ToList();
            foreach (Tile tile in flaggedTiles)
            {
                tile.ToggleFlag();
            }
            FlagAllText.text = "Flag All";
        }
        FlaggedAll = !FlaggedAll;
    }

    bool sure = false;
    public void Exit()
    {
        if (FM.playing && !sure)
        {
            ExitText.text = "Are you sure?";
            sure = true;
            Timer.SimpleTimer(() => { sure = false; ExitText.text = "Exit"; }, 5);
        }
        else
        {
            ExitText.text = "Exit";
            sure = false;
            MainMenuAI.MM.PutBack();
            foreach (Transform child in FM.TileParent) Destroy(child.gameObject);
            StopTimer();

            FlaggedAll = false;
            FlagAllText.text = "Flag All";
        }
    }
}
