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

public class InEditorMenuAI : MonoBehaviour
{
    [SerializeField] Text Height;
    [SerializeField] Text Width;
    [SerializeField] Text Mines;
    [SerializeField] Text ExitText;
    public int height;
    public int width;
    public int mines;
    public static InEditorMenuAI IEM;
    private void Awake()
    {
        IEM = this;
    }

}