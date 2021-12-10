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

public class EditorTile : MonoBehaviour
{
    [SerializeField] Renderer Cover;
    [SerializeField] TextMeshPro Number;
    [SerializeField] Renderer Mine;
    [SerializeField] Renderer Flag;
    public bool revealed = false;
    public bool Flagged = false;

    [SerializeField] bool _ContainsMine = false;
    public List<EditorTile> NearbyTiles = new List<EditorTile>();
    public int NearbyMines = 0;

    public bool ContainsMine
    {
        get { return _ContainsMine; }
        set
        {
            Mine.enabled = value;
            Number.enabled = !value;
            _ContainsMine = value;
            InEditorMenuAI.IEM.mines+=value?1:-1;
            foreach (var tile in NearbyTiles)
            {
                if (value) tile.IncrimentMines();
                else tile.DecrimentMines();
            }
        }
    }
    public Vector3 pos => transform.position;
    public Vector3 Lpos => transform.localPosition;

    public void IncrimentMines()
    {
        NearbyMines++;
        Number.text = NearbyMines + "";
        Number.color = FM.NumberColors[NearbyMines];
    }

    public void DecrimentMines()
    {
        NearbyMines--;
        Number.text = NearbyMines + "";
        Number.color = FM.NumberColors[NearbyMines];
    }
    private void Start()
    {
        NearbyTiles = GetNearbyTiles();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ToggleCovor();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ToggleFlag();
        }
        else if (Input.GetMouseButtonDown(2))
        {
            ContainsMine = !ContainsMine;
        }
    }

    public void ToggleCovor()
    {
        if (Flagged||ContainsMine) return;
        revealed = !revealed;
        Cover.enabled = !Cover.enabled;
    }

    public void ToggleFlag()
    {
        if (revealed) return;
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

    public List<EditorTile> GetNearbyTiles()
    {
        List<EditorTile> nearbyTiles = new List<EditorTile>();
        var surrounding = Physics2D.OverlapCircleAll(pos, 1);
        foreach (var collider in surrounding)
        {
            EditorTile tile = collider.GetComponent<EditorTile>();
            if (!tile.ContainsMine) nearbyTiles.Add(tile);
        }
        return nearbyTiles;
    }
}
