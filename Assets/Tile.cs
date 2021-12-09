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
using TMPro;
using static FrontMan;

public class Tile : MonoBehaviour
{
    [SerializeField] Renderer Cover;
    [SerializeField] TextMeshPro Number;
    [SerializeField] Renderer Mine;
    [SerializeField] Renderer Flag;
    public bool revealed = false;
    public bool Flagged = false;

    [SerializeField] bool _ContainsMine = false;
    public List<Tile> NearbyTiles = new List<Tile>();
    public int NearbyMines = 0;

    public bool ContainsMine
    {
        get { return _ContainsMine; }
        set
        {
            Mine.enabled = value;
            _ContainsMine = value;
            if (value)
            {
                var surrounding = Physics2D.OverlapCircleAll(pos, 1);
                foreach (var tile in GetNearbyTiles())
                {
                    tile.NearbyMines++;
                    //tile.GetComponent<SpriteRenderer>().color = Color.red;
                }

            }
        }
    }
    public Vector3 pos => transform.position;
    public Vector3 Lpos => transform.localPosition;

    private void Start()
    {
        NearbyTiles = GetNearbyTiles();
    }

    private void OnMouseOver()
    {
        if (FM.TilesRevealed == 0 || !FM.playing || InGameMenuAI.IGM.settingsOut) return;  //Returns if the board hasnt been generated yet, or the game is over
        if (Input.GetMouseButtonDown(0))
        {
            if (!revealed) Reveal(0);
            else CheckNearbyLeftClick();         //if the tile is already revealed, commit a quick reveal
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (!revealed) ToggleFlag();
            else CheckNearbyRightClick();       //if the tile is already revealed, commit a quick flag
        }
        //if (Input.GetMouseButtonDown(2)) Highlight();
    }

    public void Reveal(int recursion)  //A recursion parameter is added as if a large amount of revealing happened at once it would cause a stack overflow
    {
        if (Flagged || revealed) return;  //Flagging a tiles prevents acciedentally revealing it
        revealed = true;
        if(Cover) Cover.enabled = false;
        if (!ContainsMine)
        {
            FM.TilesRevealed++;
            if (NearbyMines > 0)
            {
                Number.text = NearbyMines + "";
                Number.color = FM.NumberColors[NearbyMines];
            }
            else  //if the tile is not near a mine, reveal ALL surrounding un-revealed tiles
            {
                if (recursion < 5) foreach (Tile tile in NearbyTiles) tile.Reveal(recursion++);
                else StartCoroutine(WaitToReveal());
            }
        }
        else  //if it does contain a mine, dont worry about anything else, just initiate a lost
        {
            if (!FM.playing) return;
            InGameMenuAI.IGM.StopTimer();
            print("LOSER!!!");
            FM.playing = false;
            Timer.SimpleTimer(() => { foreach (Tile tile in FM.tiles) tile.Reveal(0); }, 2);
        }
    }

    IEnumerator WaitToReveal()
    {
        yield return 0;
        foreach (Tile tile in NearbyTiles) tile.Reveal(0);
    }

    void CheckNearbyLeftClick()
    {
        List<Tile> FlaggedTiles = NearbyTiles.Where(T => T.Flagged).ToList();
        List<Tile> UnFlaggedTiles = NearbyTiles.Where(T => !T.Flagged).ToList();
        if (FlaggedTiles.Count == NearbyMines) foreach (var tile in UnFlaggedTiles) tile.Reveal(0);
    }
    void CheckNearbyRightClick()
    {
        List<Tile> UnrevealedTiles = NearbyTiles.Where(T => !T.revealed && !T.Flagged).ToList();
        int FlaggedTiles = NearbyTiles.Where(T => T.Flagged).Count();
        if (UnrevealedTiles.Count + FlaggedTiles == NearbyMines) foreach (var tile in UnrevealedTiles) tile.ToggleFlag();
    }

    public void ToggleFlag()
    {
        if (revealed) return;
        if (Flagged) FM.TotalFlagged--;
        else FM.TotalFlagged++;
        Flagged = !Flagged;
        Flag.enabled = Flagged;
    }
    public void SetFlag(bool state)
    {
        if (revealed) return;
        Flagged = !state;
        if (Flagged) FM.TotalFlagged--;
        else FM.TotalFlagged++;
        Flagged = !Flagged;
        Flag.enabled = Flagged;
    }

    void Highlight()
    {
        FM.circleShower.transform.position = pos;
    }

    public List<Tile> GetNearbyTiles()
    {
        List<Tile> nearbyTiles = new List<Tile>();
        var surrounding = Physics2D.OverlapCircleAll(pos, 1);
        foreach (var collider in surrounding)
        {
            Tile tile = collider.GetComponent<Tile>();
            if (!tile.ContainsMine) nearbyTiles.Add(tile);
        }
        return nearbyTiles;
    }
}