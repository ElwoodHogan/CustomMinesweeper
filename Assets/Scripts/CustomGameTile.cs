using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CustomBoardManager;

public class CustomGameTile : MonoBehaviour
{
    public SpriteRenderer tile;
    [SerializeField] SpriteRenderer flag;
    [SerializeField] SpriteRenderer mine;
    [SerializeField] SpriteRenderer cover;
    public int nearbyMines = 0;

    public bool containsMine = false;
    public bool containsFlag = false;
    public bool covered = true;
    public bool revealed = false;
    public Vector3Int pos => transform.position.Floor();
    public Vector2Int pos2 => new Vector2Int((int)transform.position.x, (int)transform.position.y);

    public static HashSet<Vector2Int> TilePosits = new HashSet<Vector2Int>();
    public static Dictionary<Vector2Int, CustomGameTile> NGTs = new Dictionary<Vector2Int, CustomGameTile>();

    public void SpawnNewTile(Vector2Int posit)
    {
        if (!TilePosits.Contains(posit) && !CBM.mineLocations.Contains(posit))
        {
            TilePosits.Add(posit);
            CustomGameTile ndt = Instantiate(CBM.CGT, posit.V2IntToV3(), Quaternion.identity, CBM.customTileParent);
            NGTs.Add(posit, ndt);
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
        List<Vector2Int> nearbyUnrevealedUnflagged = nearbyUnrevealed.Where(v => !CBM.flags.Contains(v)).ToList();
        int nearbyFlagged = nearbyUnrevealed.Count - nearbyUnrevealedUnflagged.Count;
        //if (nearbyFlagged == CBM.mineLocations.Where(v => Vector2.Distance(v, pos) < 2).Count())
        if (nearbyFlagged == nearbyMines)
        {
            foreach (var vector2 in nearbyUnrevealedUnflagged)
            {
                //reveal
            }
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
        List<Vector2Int> nearbyUnrevealedUnflagged = nearbyUnrevealed.Where(v => !CBM.flags.Contains(v)).ToList();
        int nearbyFlagged = nearbyUnrevealed.Count - nearbyUnrevealedUnflagged.Count;
        if (nearbyUnrevealed.Count == nearbyMines)
        {
            foreach (var vector2 in nearbyUnrevealedUnflagged)
            {
                //CBM.CreateFlag(vector2);
            }
        }
    }
}
