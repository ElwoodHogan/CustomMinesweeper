using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FrontMan;

public class NewGameMenuAI : MonoBehaviour
{
    [SerializeField] InputField Height;
    [SerializeField] InputField Width;
    [SerializeField] InputField Mines;
    [SerializeField] InputField ExtraLives;
    public Toggle SafetyMode;
    [SerializeField] int height = 9;
    [SerializeField] int width = 9;
    [SerializeField] int mines = 15;
    [SerializeField] int extraLives = 0;
    [SerializeField] TextMeshProUGUI Warning;
    [SerializeField] TextMeshProUGUI ReccomendedMines;

    public static NewGameMenuAI NGM;
    private void Awake()
    {
        NGM = this;
    }
    private void Start()
    {
        string saturationWarning = "WARNING: This map is heavily saturated with mines, meaning mine generation will use a secondary algorithm, which takes much longer to generate at larger map sizes (i.e. 400x400+).  " +
                    "For quick generation, use at most ";
        Height.onValueChanged.AddListener((s) =>
        {
            if (!isNumeric(s)) Warning.text = "Only whole numbers in the input field pls.";
            else
            {
                int.TryParse(s, out height); Warning.text = "";
                if (isNumeric(Width.text))
                {
                    ReccomendedMines.text = "Reccomended amount of mines for board size: " + Mathf.Floor((width * height) * .18f);
                }
                if (mines > (width * height * .995) + 1)
                {
                    Warning.text = saturationWarning + Mathf.Floor((float)(width * height * .995) + 1) + " mines";
                }else if(width * height >= (2500*2500))
                {
                    Warning.text = "Look, one of the things I wanted to do with this game was allow as big a board size as you wanted.  But computers have limitations, and a board size that big will cause general gameplay lag.  So consider this a warning.";
                }
            }
        });
        Width.onValueChanged.AddListener((s) =>
        {
            if (!isNumeric(s)) Warning.text = "Only whole numbers in the input field pls.";
            else
            {
                int.TryParse(s, out width); Warning.text = "";
                if (isNumeric(Height.text))
                {
                    ReccomendedMines.text = "Reccomended amount of mines for board size: " + Mathf.Floor((width * height) * .18f);
                }
            }
            if (mines > (width * height * .995) + 1)
            {
                Warning.text = saturationWarning + Mathf.Floor((float)(width * height * .995) + 1) + " mines";
            }
            else if (width * height >= (2500 * 2500))
            {
                Warning.text = "Look, one of the things I wanted to do with this game was allow as big a board size as you wanted.  But computers have limitations, and a board size that big will cause general gameplay lag.  So consider this a warning.";
                }
        });
        Mines.onValueChanged.AddListener((s) =>
        {
            if (!isNumeric(s)) Warning.text = "Only whole numbers in the input field pls.";
            else
            {
                int.TryParse(s, out mines); Warning.text = "";
                if (mines > (width * height * .995) + 1)
                {
                    Warning.text = saturationWarning + Mathf.Floor((float)(width * height * .995) + 1) + " mines";
                }
            }
        });
        ExtraLives.onValueChanged.AddListener((s) =>
        {
            if (!isNumeric(s)) Warning.text = "Only whole numbers in the input field pls.";
            else
            {
                int.TryParse(s, out extraLives); Warning.text = "";
            }
        });
    }
    private void Update()
    {
        //ConsoleProDebug.Watch("hashset method?", (mines <= (height*width * .995) + 1) + "");
    }

    public void SetBoard()
    {
        if (!isNumeric(Height.text) || !isNumeric(Width.text) || !isNumeric(Mines.text))
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
            Warning.text = $"Too many mines!  Max mines must be at most 9 less than the total tiles!  For your current board size: {TotalTiles}, your minimum amount of mines is {TotalTiles - 9}.";
            return;
        }
        if (TotalTiles < 10 && SafetyMode.isOn)
        {
            Warning.text = "Board must have at least 10 total tiles!";
            return;
        }
        if (mines > TotalTiles)
        {
            Warning.text = $"More mines that total tiles!  You have {TotalTiles} total tiles.  The amount of mines must be less than or equal to that";
            return;
        }
        FM.SetBoard(height, width, mines);
        FM.extraLives = extraLives;
        MainMenuAI.MM.PutAway();
        InGameMenuAI.IGM.resetTimer();
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
                SafetyMode.isOn = true;
                Height.text = 9 + "";
                Width.text = 9 + "";
                Mines.text = 12 + "";
                ExtraLives.text = 0 + "";
                break;
            case 1:
                SafetyMode.isOn = true;
                Height.text = 16 + "";
                Width.text = 16 + "";
                Mines.text = 40 + "";
                ExtraLives.text = 0 + "";
                break;
            case 2:
                SafetyMode.isOn = true;
                Height.text = 16 + "";
                Width.text = 30 + "";
                Mines.text = 99 + "";
                ExtraLives.text = 0 + "";
                break;
            case 3:
                SafetyMode.isOn = true;
                Height.text = 100 + "";
                Width.text = 100 + "";
                Mines.text = 3500 + "";
                ExtraLives.text = 1 + "";
                break;
            case 4:
                SafetyMode.isOn = false;
                Height.text = 1 + "";
                Width.text = 1 + "";
                Mines.text = 1 + "";
                ExtraLives.text = 0 + "";
                break;
        }
    }
}
