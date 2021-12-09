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

public class CameraAI : MonoBehaviour
{
    [SerializeField] Vector3 mouseLastFrame;
    [SerializeField] Camera cam;

    private void Update()
    {
        Vector2 mouseChange = Camera.main.ScreenToWorldPoint(Input.mousePosition) - mouseLastFrame;
        if (Input.GetMouseButton(2)) Camera.main.transform.Translate(-mouseChange);  //Pans the camera
        mouseLastFrame = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - Input.mouseScrollDelta.y/1.5f, 2, 20);
    }
}
