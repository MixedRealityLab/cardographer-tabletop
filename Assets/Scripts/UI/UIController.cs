using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Context Menus")]
    public GameObject AnnotationEdit;
    public GameObject CameraControl;
    public GameObject CardUiMenu;
    public GameObject DeckUiMenu;
    public GameObject BoardUiMenu;
    public GameObject ActiveMenu;

    [Header("Option Menu")]
    public GameObject OptionsMenu;
    bool OptionsActive;

    [Header("Preview Camera")]
    public Camera PreviewCamera;
    public GameObject PreviewUI;

    bool UIActive = false;
    Camera LocalCamera;

    public void MoveAndAdjustPreviewCamera(Vector3 pos)
    {
        if (!PreviewCamera.gameObject.activeInHierarchy) PreviewCamera.gameObject.SetActive(true);
        if (!PreviewUI.activeInHierarchy) PreviewUI.SetActive(true);
        pos.y += 2;
        PreviewCamera.gameObject.transform.position = pos;
    }

    public void StopPreview()
    {
        PreviewUI.SetActive(false);
    }

    public void ToggleOptionsMenu()
    {
        OptionsActive = !OptionsMenu.activeInHierarchy;
        OptionsMenu.SetActive(OptionsActive);
    }

    public void ToggleCardMenu(Vector3 pos, GameObject card = null)
    {
        if(pos == Vector3.zero)
        {
            CardUiMenu.SetActive(false);
            ActiveMenu = null;
        }
        else if (ActiveMenu != CardUiMenu) CardUiMenu.SetActive(!CardUiMenu.activeInHierarchy);

        if (CardUiMenu.activeInHierarchy)
        {
            CardUiMenu.transform.position = LocalCamera.WorldToScreenPoint(pos);
            if (ActiveMenu != null && ActiveMenu != CardUiMenu) ActiveMenu.SetActive(false);
            ActiveMenu = CardUiMenu;
            if (card != null) CardUiMenu.GetComponent<CardUiMenu>().SetTarget(card);
            UIActive = true;
        }
        else
        {
            UIActive = false;
            if(ActiveMenu != null) ActiveMenu.SetActive(UIActive);
        } 
            
        SwitchControls(UIActive);
    }

    public void ToggleDeckMenu(Vector3 pos, GameObject deck = null)
    {
        if (pos == Vector3.zero)
        {
            DeckUiMenu.SetActive(false);
            ActiveMenu = null;
        }
        else if (ActiveMenu != DeckUiMenu) DeckUiMenu.SetActive(!DeckUiMenu.activeInHierarchy);
        if (DeckUiMenu.activeInHierarchy)
        {
            DeckUiMenu.transform.position = LocalCamera.WorldToScreenPoint(pos);
            if (ActiveMenu != null && ActiveMenu != DeckUiMenu) ActiveMenu.SetActive(false);
            ActiveMenu = DeckUiMenu;
            if (deck != null) DeckUiMenu.GetComponent<DeckUiMenu>().SetTarget(deck);
            UIActive = true;
        }
        else
        {
            UIActive = false;
            if (ActiveMenu != null) ActiveMenu.SetActive(UIActive);
        }
        SwitchControls(UIActive);
    }

    public void ToggleBoardMenu(Vector3 pos, GameObject board = null)
    {
        if (pos == Vector3.zero)
        {
            BoardUiMenu.SetActive(false);
            ActiveMenu = null;
        }
        else if (ActiveMenu != BoardUiMenu) BoardUiMenu.SetActive(!BoardUiMenu.activeInHierarchy);

        if (BoardUiMenu.activeInHierarchy)
        {
            BoardUiMenu.transform.position = LocalCamera.WorldToScreenPoint(pos);
            if (ActiveMenu != null && ActiveMenu != BoardUiMenu) ActiveMenu.SetActive(false);
            ActiveMenu = BoardUiMenu;
            if (board != null) BoardUiMenu.GetComponent<BoardUiMenu>().SetTarget(board);
            UIActive = true;
        }
        else
        {
            UIActive = false;
            if (ActiveMenu != null) ActiveMenu.SetActive(UIActive);
        }
        SwitchControls(UIActive);
    }

    public void ToggleAnnotationEdit(Vector3 pos, GameObject anno = null)
    {
        if (pos == Vector3.zero)
        {
            AnnotationEdit.SetActive(false);
            ActiveMenu = null;
        }
        else if (ActiveMenu != AnnotationEdit) AnnotationEdit.SetActive(!AnnotationEdit.activeInHierarchy);
        if (AnnotationEdit.activeInHierarchy)
        {
            AnnotationEdit.transform.position = LocalCamera.WorldToScreenPoint(pos);
            if (ActiveMenu != null && ActiveMenu != AnnotationEdit) ActiveMenu.SetActive(false);
            ActiveMenu = AnnotationEdit;
            if(anno != null) AnnotationEdit.GetComponent<AnnotationEdit>().SetTarget(anno);
            UIActive = true;
        }
        else
        {
            UIActive = false;
            if (ActiveMenu != null) ActiveMenu.SetActive(UIActive);
        }
        SwitchControls(UIActive);
    }

    public void CloseUi()
    {
        UIActive = false;
        if (ActiveMenu != null)
        {
            ActiveMenu.SetActive(UIActive);
            ActiveMenu = null;
        }
        SwitchControls(false);
    }

    void SwitchControls(bool active)
    {
        CameraControl.GetComponent<SwivelStickCamera>().enabled = !active;
    }

    public bool GetUIState() { return UIActive; }

    // Start is called before the first frame update
    void Start()
    {
        LocalCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
