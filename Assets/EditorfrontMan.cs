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

public class EditorfrontMan : MonoBehaviour
{
    [SerializeField] int height;
    [SerializeField] int width;
    [SerializeField] int mines;
    [SerializeField] EditorTile Etile;
    public Transform ETileParent;
    public bool Eplaying;
    public List<EditorTile> Etiles;
    public static FrontMan EFM;
    public int ETilesRevealed = 0;
    public int ETotalFlagged = 0;
    public int ETotalTiles = 0;

    public void SetEditorBoard(int _height, int _width, int _mines)
    {
        Eplaying = true;
        GameObject TileParentTemp = new GameObject();
        ETileParent = TileParentTemp.transform;
        height = _height;
        width = _width;
        mines = _mines;
        Eplaying = true;
        InGameMenuAI.IGM.Init(height, width, mines);
        ETotalTiles = height * width;
        ETotalFlagged = 0;
        ETilesRevealed = 0;
        Etiles = new List<EditorTile>();
        foreach (Transform child in ETileParent) Destroy(child.gameObject);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Etiles.Add(Instantiate(Etile, new Vector2(x, y), Quaternion.identity, ETileParent));  //Spawning grid
                Etiles[Etiles.Count - 1].name = $"Tile x:{x}, y:{y}";
            }
        }

        List<EditorTile> tempList = new List<EditorTile>(Etiles);
        int tempMines = mines;
        while (tempMines > 0)
        {
            int i = UnityEngine.Random.Range(0, tempList.Count);
            tempList[i].ContainsMine = true;
            tempList.RemoveAt(i);
            tempMines--;
        }


        Camera.main.transform.position = new Vector3(width / 2, height / 2, -10);
        Camera.main.orthographicSize = Mathf.Clamp((Mathf.Max(height, width) / 2) + 3, 2, SettingMenuAI.SM.GetCC() ? 20 : 999);  //Scales the camera to envolope larger maps
    }

}
