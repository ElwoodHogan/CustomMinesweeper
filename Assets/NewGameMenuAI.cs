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
using TMPro;

public class NewGameMenuAI : MonoBehaviour
{
    [SerializeField] InputField Height;
    [SerializeField] InputField Width;
    [SerializeField] InputField Mines;
    public Toggle SafetyMode;
    [SerializeField] int height = 9;
    [SerializeField] int width = 9;
    [SerializeField] int mines = 15;
    [SerializeField] TextMeshProUGUI Warning;
    public static NewGameMenuAI NGM;
    private void Awake()
    {
        NGM = this;
    }
    private void Start()
    {
        Height.onValueChanged.AddListener((s)=> { 
            if (!isNumeric(s)) Warning.text = "Only whole numbers in the input field pls."; 
            else {int.TryParse(s, out height); Warning.text = "";
                if (height * width > 4000) Warning.text = "Wooooaaaaah big numbers there.  Look, one of the main things I wanted to achieve with this game is to allow you to make as big of a board as you want.  HOWEVER:  browser based programs have their limits.  I have personally tested up to 500x500, and that took about 4 minutes to create, and it crashed when exiting, so consider this a warning.";
            } });
        Width.onValueChanged.AddListener((s) => { 
            if (!isNumeric(s)) Warning.text = "Only whole numbers in the input field pls."; 
            else {int.TryParse(s, out width); Warning.text = "";
                if (height * width > 4000) Warning.text = "Wooooaaaaah big numbers there.  Look, one of the main things I wanted to achieve with this game is to allow you to make as big of a board as you want.  HOWEVER:  browser based programs have their limits.  I have personally tested up to 500x500, and that took about 4 minutes to create, and it crashed when exiting, so consider this a warning.";
            } });
        Mines.onValueChanged.AddListener((s) => { if (!isNumeric(s)) Warning.text = "Only whole numbers in the input field pls."; else { int.TryParse(s, out mines); Warning.text = ""; } });
    }


    public void SetBoard()
    {
        if(!isNumeric(Height.text) || !isNumeric(Width.text) || !isNumeric(Mines.text))
        {
            Warning.text = "Only whole numbers in the input field pls.";
            return;
        }
        if (height <= 0 || width <= 0)
        {
            Warning.text = "Height and Width must be at least 1";
            return;
        }
        if (mines < 0)
        {
            Warning.text = "Mines must be at least 0";
            return;
        }
        int TotalTiles = height * width;
        if (TotalTiles - 9 < mines && SafetyMode.isOn)
        {
            Warning.text = $"Too many mines!  Max mines must be at most 9 less than the total tiles!  For your current board size: {TotalTiles}, your minimum amount of mines is {TotalTiles-9}.";
            return;
        }
        if (TotalTiles < 10 && SafetyMode.isOn)
        {
            Warning.text = "Board must have at least 10 total tiles!";
            return;
        }
        if(mines > TotalTiles)
        {
            Warning.text = $"More mines that total tiles!  You have {TotalTiles} total tiles.  The amount of mines must be less than or equal to that";
            return;
        }
        EditorfrontMan.EFM.SetBoard(height, width, mines);
        MainMenuAI.MM.PutAway();
    }

    public bool isNumeric(string s)
    {
        float output = 0;
        if (s == null)
        {
            return false;
        }
        return (float.TryParse(s, out output));
    }

    public void SetTemplate(int index)
    {
        SafetyMode.isOn = false;
        switch (index)
        {
            case 0:
                Height.text = 9 + "";
                Width.text = 9 + "";
                Mines.text = 15+"";
                break;
            case 1:
                Height.text = 16 + "";
                Width.text = 16 + "";
                Mines.text = 40 + "";
                break;
            case 2:
                Height.text = 30 + "";
                Width.text = 16 + "";
                Mines.text = 99 + "";
                break;
            case 3:
                Height.text = 100 + "";
                Width.text = 100 + "";
                Mines.text = 3500 + "";
                break;
            case 4:
                SafetyMode.isOn = false;
                Height.text = 1 + "";
                Width.text = 1 + "";
                Mines.text = 1 + "";
                break;
        }
    }
}
