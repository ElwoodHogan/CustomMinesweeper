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

public class Tester : MonoBehaviour
{
    public int rows, cols, gap;
    public GameObject tileP;

    [Button]
    public void doSpawn()
    {
        int count = 0;
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Reset();
        stopwatch.Start();

        Vector3 loc = Vector3.zero;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                loc.y = (r * gap) - ((rows * gap) / 2) + (gap / 2);
                loc.x = (c * gap) - ((cols * gap) / 2) + (gap / 2);

                GameObject tile = Instantiate(tileP, loc, Quaternion.identity);
                count++;
            }
        }
        stopwatch.Stop();
        Debug.Log("Objects:" + count + " - " + "Time:" + stopwatch.Elapsed);
    }


}
