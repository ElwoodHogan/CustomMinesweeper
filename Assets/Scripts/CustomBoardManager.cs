using System.Collections.Generic;
using UnityEngine;

public class CustomBoardManager : MonoBehaviour
{
    public static CustomBoardManager CBM;
    public Transform customTileParent;

    public int TilesRevealed = 0;
    public HashSet<Vector2Int> mineLocations;
    public HashSet<Vector2Int> flags;
    public HashSet<Vector2Int> DeletedTiles;
    public CustomGameTile CGT;

    private void Awake()
    {
        CBM = this;
    }
}
