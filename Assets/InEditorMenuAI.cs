using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static EditorfrontMan;

public class InEditorMenuAI : MonoBehaviour
{
    [SerializeField] Text Height;
    [SerializeField] Text Width;
    [SerializeField] Text Mines;
    [SerializeField] Text ExitText;

    [SerializeField] Image MineToolSelect;
    [SerializeField] Image FlagToolSelect;
    [SerializeField] Image RevealToolSelect;
    [SerializeField] Image CreateToolSelect;
    [SerializeField] Image DestroyToolSelect;
    [SerializeField] Color SelectColor;
    Color defaultColor;
    editorTools lastFrameSelectedTool;
    public static InEditorMenuAI IEM;
    private void Awake()
    {
        IEM = this;
        defaultColor = MineToolSelect.color;
    }
    private void Update()
    {
        if (!EFM.editing) return;
        Height.text = EFM.height + "";
        Width.text = EFM.width + "";
        Mines.text = EFM.mines + "";

        switch (lastFrameSelectedTool)
        {
            case editorTools.mine:
                MineToolSelect.color = defaultColor;
                break;
            case editorTools.flag:
                FlagToolSelect.color = defaultColor;
                break;
            case editorTools.reveal:
                RevealToolSelect.color = defaultColor;
                break;
            case editorTools.create:
                CreateToolSelect.color = defaultColor;
                break;
            case editorTools.destroy:
                DestroyToolSelect.color = defaultColor;
                break;
        }

        switch (EFM.currentTool)
        {
            case editorTools.mine:
                MineToolSelect.color = SelectColor;
                break;
            case editorTools.flag:
                FlagToolSelect.color = SelectColor;
                break;
            case editorTools.reveal:
                RevealToolSelect.color = SelectColor;
                break;
            case editorTools.create:
                CreateToolSelect.color = SelectColor;
                break;
            case editorTools.destroy:
                DestroyToolSelect.color = SelectColor;
                break;
        }
        lastFrameSelectedTool = EFM.currentTool;
    }
}