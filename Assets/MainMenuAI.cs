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
using DG.Tweening;

public class MainMenuAI : MonoBehaviour
{
    public Transform gotoPoint;
    [SerializeField] Transform leavePoint;
    [SerializeField] Transform ButtonMenuGoToPoint;
    [SerializeField] Transform ButtonMenuLeavePoint;
    [SerializeField] Transform MainMenuLeavePoint;
    [SerializeField] Transform ButtonMenu;
    [SerializeField] Transform SelectedMenu = null;
    public Transform SettingsMenu;
    [SerializeField] List<Transform> MenuList = new List<Transform>();

    Vector3 MMOGPoint;  //Storing the original positions of the menus to put them back
    Vector3 BMOGPoint;
    Vector3 SMOGPoint;
    public static MainMenuAI MM;
    private void Awake()
    {
        MM = this;
    }
    public void SwitchMenu(int menuIndex)
    {
        ButtonMenu.DOMove(ButtonMenuGoToPoint.position, 1);
        if (SelectedMenu == MenuList[menuIndex]) return;
        if (SelectedMenu) SelectedMenu.DOMove(leavePoint.position, 1);
        SelectedMenu = MenuList[menuIndex];
        SelectedMenu.DOMove(gotoPoint.position, 1);
    }

    public void PutAway()
    {
        MMOGPoint = transform.position;
        BMOGPoint = ButtonMenu.position;
        SMOGPoint = SelectedMenu.position;
        ButtonMenu.DOMove(ButtonMenuLeavePoint.position, 1);
        transform.DOMove(MainMenuLeavePoint.position, 1);
        SelectedMenu.DOMove(leavePoint.position, 1);
        InGameMenuAI.IGM.transform.DOMove(InGameMenuAI.IGM.transform.position.Change(0, 1080, 0), 1);
    }
    public void PutBack()
    {
        ButtonMenu.DOMove(ButtonMenuGoToPoint.position, 1);
        transform.DOMove(MMOGPoint, 1);
        SelectedMenu.DOMove(SMOGPoint, 1);
        SettingsMenu.DOMove(leavePoint.position, 1);
        InGameMenuAI.IGM.transform.DOMove(InGameMenuAI.IGM.transform.position.Change(0, -1080, 0), 1);
    }
}
