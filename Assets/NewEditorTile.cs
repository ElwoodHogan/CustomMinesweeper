using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EditorfrontMan;

public class NewEditorTile : MonoBehaviour
{
    public SpriteRenderer tile;
    [SerializeField] SpriteRenderer mine;
    [SerializeField] SpriteRenderer flag;
    [SerializeField] SpriteRenderer cover;

    public bool containsMine = false;
    public bool containsFlag = false;
    public bool covered = true;
    public bool revealed = false;
    public Vector3Int pos => transform.position.Floor();
    public Vector2Int pos2 => new Vector2Int((int)transform.position.x, (int)transform.position.y);

    public static HashSet<Vector2Int> TilePosits = new HashSet<Vector2Int>();
    public static Dictionary<Vector2Int, NewEditorTile> NETs = new Dictionary<Vector2Int, NewEditorTile>();

    public int nMines = 0;
    public int nearbyMines
    {
        get { return nMines; }
        set
        {
            nMines = value;
            number.text = nMines > 0 ? nMines + "" : "";
            number.color = FrontMan.FM.NumberColors[nMines];
        }
    }

    [SerializeField] TextMeshPro number;
    private void Start()
    {
        name = $"OST x:{pos.x} y:{pos.y}";  //COMMENT OUT POST-FINAL
        StartCoroutine(SpawnWhileInScreen());
    }

    public void ToggleMine()
    {
        containsMine = !containsMine;
        mine.enabled = containsMine;
        if (containsMine) number.text = "";
        foreach (var tile in NearbyTiles()) tile.nearbyMines += containsMine ? 1 : -1;
    }
    public void ToggleFlag()
    {
        containsFlag = !containsFlag;
        flag.enabled = containsFlag;
    }
    public void ToggleCover()
    {
        covered = !covered;
        cover.enabled = covered;
    }

    IEnumerator SpawnWhileInScreen()
    {
        while (!tile.isVisible)
            yield return null;

        for (int x = pos.x - 1; x <= pos.x + 1; x++)
            for (int y = pos.y - 1; y <= pos.y + 1; y++)
                SpawnEditorTile(new Vector2Int(x, y));
    }
    /*
    public void SpawnNewTile(Vector2Int posit)
    {
        if (TilePosits.Add(posit))
        {
            NewEditorTile ndt = Instantiate(this, posit.V2IntToV3(), Quaternion.identity, EFM.TileParent);
            NETs.Add(posit, ndt);
        }
    }

    public static void SpawnNewTile(Vector2Int posit, NewEditorTile net)
    {
        if (TilePosits.Add(posit))
        {
            NewEditorTile ndt = Instantiate(net, posit.V2IntToV3(), Quaternion.identity, EFM.TileParent);
            NETs.Add(posit, ndt);
        }
    }*/

    public List<NewEditorTile> NearbyTiles()
    {
        return NETs.Where(pair => Vector2Int.Distance(pair.Key, pos2) < 2).Select(kvp => kvp.Value).ToList();
    }
}
