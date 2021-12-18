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
    [SerializeField] Text TogglemodeText;
    [SerializeField] GameObject ShiftmodeDisplayer;
    [SerializeField] Text ShiftmodeToggleText;

    [SerializeField] Image MineToolSelect;
    [SerializeField] Image FlagToolSelect;
    [SerializeField] Image RevealToolSelect;
    [SerializeField] Image CreateToolSelect;
    [SerializeField] Image DestroyToolSelect;
    [SerializeField] Color SelectColor;

    [SerializeField] GameObject ToolMenu;

    [SerializeField] Transform controlsMenu;
    [SerializeField] Transform beginPoint;
    [SerializeField] Transform endPoint;
    bool controlsMenuUp = false;
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
        TogglemodeText.text = EFM.ToggleOrSet ? "Toggle" : "Set";
        if (!EFM.ToggleOrSet)
        {
            ShiftmodeDisplayer.SetActive(true);
            ShiftmodeToggleText.text = EFM.AddOrClear ? "Add" : "Clear";
        }else ShiftmodeDisplayer.SetActive(false);


        if (Input.GetKeyDown(KeyCode.T))
        {
            toggleControlsMenus();
        }



        if (!ToolMenu.activeSelf) return;
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


    public void toggleToolMenu()
    {
        ToolMenu.SetActive(!ToolMenu.activeSelf);
    }

    public void toggleControlsMenus()
    {
        if (!controlsMenuUp)
        {
            controlsMenu.transform.DOMove(transform.position, .5f);
            controlsMenuUp = true;
        }
        else
        {
            controlsMenu.transform.DOMove(endPoint.position, .5f).OnComplete(() => controlsMenu.position = beginPoint.position);
            controlsMenuUp = false;
        }
            
    }
}