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

    public bool ToggleMine()
    {
        containsMine = !containsMine;
        mine.enabled = containsMine;
        if (containsMine) number.text = "";
        foreach (var tile in NearbyTiles()) tile.nearbyMines += containsMine ? 1 : -1;
        return true;
    }

    public bool SetMine(bool mineState)
    {
        if(containsMine = mineState) return false;
        containsMine = mineState;
        mine.enabled = mineState;
        return true;
    }
    public bool ToggleFlag()
    {
        containsFlag = !containsFlag;
        flag.enabled = containsFlag;
        return true;
    }
    public bool SetFlag(bool flagState)
    {
        if (containsFlag == flagState) return false;
        containsFlag = flagState;
        flag.enabled = flagState;
        return true;
    }
    public bool ToggleCover()
    {
        covered = !covered;
        revealed = !covered;
        cover.enabled = covered;
        return true;
    }

    public bool SetCover(bool coverState)
    {
        if (covered == coverState) return false;
        covered = coverState;
        revealed = !coverState;
        cover.enabled = coverState;
        return true;
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
