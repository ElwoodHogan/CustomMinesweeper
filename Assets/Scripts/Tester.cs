using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tester : MonoBehaviour
{
    public int rows, cols, gap;
    public GameObject tileP;

    [Button]

    public void Do()
    {
        StartCoroutine(doSpawn2());
    }
    IEnumerator doSpawn(int width = 500, int height = 500)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        int TotalTiles = width * height;
        int max = (int)(TotalTiles * .98f);

        int run = 0;
        print(max);
        while (stopwatch.Elapsed.TotalSeconds < 5)
        {
            max += 50;
            run++;
            print("Run: " + run + " " + max);
            int mines = max;

            stopwatch.Reset();
            stopwatch.Start();
            HashSet<Vector2Int> mineLocations = new HashSet<Vector2Int>();
            Vector2Int gridSize = new Vector2Int(width, height);
            while (mineLocations.Count < mines)
            {
                mineLocations.Add(new Vector2Int(UnityEngine.Random.Range(1, gridSize.x), UnityEngine.Random.Range(1, gridSize.y)));
            }

            stopwatch.Stop();
            print(run + " " + stopwatch.Elapsed);
            yield return 0;
        }



    }
    IEnumerator doSpawn2(int width = 500, int height = 500)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        int TotalTiles = width * height;
        //int max = (int)(TotalTiles * .98f);
        int max = 248500;

        int run = 0;
        print(max);
        for (int i = 0; i < 8; i++)
        {
            max += 50;
            run++;
            print("Run: " + run + " " + max);
            int mines = max;

            stopwatch.Reset();
            stopwatch.Start();
            HashSet<Vector2Int> mineLocations = new HashSet<Vector2Int>();
            Vector2Int gridSize = new Vector2Int(width, height);
            while (mineLocations.Count < mines)
            {
                mineLocations.Add(new Vector2Int(UnityEngine.Random.Range(1, gridSize.x), UnityEngine.Random.Range(1, gridSize.y)));
            }

            stopwatch.Stop();
            print(run + " " + stopwatch.Elapsed);
            yield return 0;
        }



    }
}
