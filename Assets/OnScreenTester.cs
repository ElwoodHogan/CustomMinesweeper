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

public class OnScreenTester : MonoBehaviour
{
    public SpriteRenderer tile;
    public Vector3 pos => transform.position;
    public Vector2 pos2 => new Vector3(transform.position.x, transform.position.y);
    public static HashSet<Vector2> TilePosits = new HashSet<Vector2>();
    public HashSet<Vector2> PTilePosits = new HashSet<Vector2>();

    public int nearbyMines = 0;

    public bool wait = true;

    [SerializeField] TextMeshPro number;

    [Button]
    public void FakeStart()
    {
        StartCoroutine(SpawnAboveWhileInScreen());
        TilePosits.Add(pos2);
    }
    private void Start()
    {
        if (wait) return;
        FM.TilesRevealed++;
        nearbyMines = FM.mineLocations.Where(v => Vector2.Distance(v, pos2) < 2).Count();
        if (nearbyMines == 0)
        {
            StartCoroutine(SpawnAboveWhileInScreen());
            TilePosits.Add(pos2);
        }
        else
        {
            TextMeshPro n = Instantiate(number, transform);
            n.text = nearbyMines+"";
            n.color = FM.NumberColors[nearbyMines];
        }
        
    }
    private void Update()
    {
        PTilePosits = TilePosits;
    }

    IEnumerator SpawnAboveWhileInScreen()
    {
        while (!tile.isVisible)
        {
            yield return null;
        }
        if(TilePosits.Add(new Vector2(pos.x+1, pos.y))) Instantiate(gameObject, new Vector2(pos.x + 1, pos.y), Quaternion.identity, FM.TileParent);
        if (TilePosits.Add(new Vector2(pos.x + 1, pos.y+1))) Instantiate(gameObject, new Vector2(pos.x + 1, pos.y+1), Quaternion.identity, FM.TileParent);
        if (TilePosits.Add(new Vector2(pos.x-1, pos.y))) Instantiate(gameObject, new Vector2(pos.x - 1, pos.y), Quaternion.identity, FM.TileParent);
        if (TilePosits.Add(new Vector2(pos.x - 1, pos.y+1))) Instantiate(gameObject, new Vector2(pos.x - 1, pos.y+1), Quaternion.identity, FM.TileParent);
        if (TilePosits.Add(new Vector2(pos.x, pos.y+1))) Instantiate(gameObject, new Vector2(pos.x, pos.y+1), Quaternion.identity, FM.TileParent);
        if (TilePosits.Add(new Vector2(pos.x + 1, pos.y-1))) Instantiate(gameObject, new Vector2(pos.x + 1, pos.y-1), Quaternion.identity, FM.TileParent);
        if (TilePosits.Add(new Vector2(pos.x, pos.y-1))) Instantiate(gameObject, new Vector2(pos.x, pos.y-1), Quaternion.identity, FM.TileParent);
        if (TilePosits.Add(new Vector2(pos.x - 1, pos.y-1))) Instantiate(gameObject, new Vector2(pos.x - 1, pos.y-1), Quaternion.identity, FM.TileParent);
    }
}
