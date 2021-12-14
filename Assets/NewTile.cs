using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FrontMan;

public class NewTile : SerializedMonoBehaviour
{
    public SpriteRenderer tile;
    public Vector3 pos => transform.position;
    public Vector2 pos2 => new Vector2(transform.position.x, transform.position.y);
    public Vector2Int pos2Int => new Vector2Int((int)transform.position.x, (int)transform.position.y);
    public static HashSet<Vector2Int> TilePosits = new HashSet<Vector2Int>();
    public static Dictionary<Vector2Int, NewTile> NewTiles;

    public int nearbyMines = 0;

    [SerializeField] TextMeshPro number;

    private void Start()
    {
        FM.TilesRevealed++;

        nearbyMines = FM.mineLocations.Where(v => Vector2Int.Distance(v, pos2Int) < 2).Count();
        name = $"OST x:{pos.x} y:{pos.y}";  //COMMENT OUT POST-FINAL
        if (nearbyMines == 0)
        {
            StartCoroutine(SpawnAboveWhileInScreen());
        }
        else
        {
            number = Instantiate(number, transform);
            number.text = nearbyMines + "";
            number.color = FM.NumberColors[nearbyMines];
        }

    }

    IEnumerator SpawnAboveWhileInScreen()
    {
        while (!tile.isVisible)
        {
            yield return null;
        }
        for (int x = (int)pos.x - 1; x <= (int)pos.x + 1; x++)
        {
            for (int y = (int)pos.y - 1; y <= (int)pos.y + 1; y++)
            {
                SpawnNewTile(new Vector2Int(x, y));
            }
        }
    }

    public void SpawnNewTile(Vector2Int posit)
    {
        if (!TilePosits.Contains(posit) && !FM.mineLocations.Contains(posit))
        {
            TilePosits.Add(posit);
            NewTile ndt = Instantiate(FM.OST, posit.V2IntToV3(), Quaternion.identity, FM.TileParent);
            NewTiles.Add(posit, ndt);
        }
    }

    public void QuickReveal()
    {
        List<Vector2Int> squareAround = new List<Vector2Int>();
        for (int x = (int)pos.x - 1; x <= pos.x + 1; x++)
        {
            for (int y = (int)pos.y - 1; y <= pos.y + 1; y++)
            {
                squareAround.Add(new Vector2Int(x, y));
            }
        }
        List<Vector2Int> nearbyUnrevealed = squareAround.Where(v => !TilePosits.Contains(v)).ToList();
        List<Vector2Int> nearbyUnrevealedUnflagged = nearbyUnrevealed.Where(v => !FM.flags.Contains(v)).ToList();
        int nearbyFlagged = nearbyUnrevealed.Count - nearbyUnrevealedUnflagged.Count;
        if (nearbyFlagged == FM.mineLocations.Where(v => Vector2.Distance(v, pos) < 2).Count())
        {
            foreach (var vector2 in nearbyUnrevealedUnflagged) SpawnNewTile(vector2);
        }
    }

    public void QuickFlag()
    {
        List<Vector2Int> squareAround = new List<Vector2Int>();
        for (int x = (int)pos.x - 1; x <= pos.x + 1; x++)
        {
            for (int y = (int)pos.y - 1; y <= pos.y + 1; y++)
            {
                squareAround.Add(new Vector2Int(x, y));
            }
        }
        List<Vector2Int> nearbyUnrevealed = squareAround.Where(v => !TilePosits.Contains(v)).ToList();
        List<Vector2Int> nearbyUnrevealedUnflagged = nearbyUnrevealed.Where(v => !FM.flags.Contains(v)).ToList();
        int nearbyFlagged = nearbyUnrevealed.Count - nearbyUnrevealedUnflagged.Count;
        if (nearbyUnrevealed.Count == FM.mineLocations.Where(v => Vector2.Distance(v, pos) < 2).Count())
        {
            foreach (var vector2 in nearbyUnrevealedUnflagged)
            {
                FM.CreateFlag(vector2);
            }
        }
    }
}
