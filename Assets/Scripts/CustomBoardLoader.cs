using System.Collections.Generic;
using UnityEngine;

public class CustomBoardLoader : MonoBehaviour
{
    public int height;
    public int width;
    public static CustomBoardLoader CBL;
    public HashSet<Vector2Int> mineLocations;
    public HashSet<Vector2Int> flags;
    public HashSet<Vector2Int> DeletedTiles;


    private void Awake()
    {
        CBL = this;
    }
}
