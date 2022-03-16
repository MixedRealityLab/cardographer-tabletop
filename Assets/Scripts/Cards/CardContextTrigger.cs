﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.UIWidgets;
using ContextMenu = DevionGames.UIWidgets.ContextMenu;
using UnityEngine.EventSystems;

public class CardContextTrigger : ContextTriggerOverride
{
    private ContextMenu m_ContextMenu;

    public GameObject cardObject;

    // Start is called before the first frame update
    private void Start()
    {
        m_ContextMenu = WidgetUtility.Find<ContextMenu>("ContextMenu");
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            m_ContextMenu.Clear();

            m_ContextMenu.AddMenuItem("Flip", delegate { 
                Debug.Log("Flip Clicked");
                cardObject.GetComponent<CardBehaviour>().CardInteract(1);
            });

            /*m_ContextMenu.AddMenuItem("Load Image", delegate {
                Debug.Log("Load Clicked");
                cardObject.GetComponent<CardBehaviour>().CardInteract(2);
            });
            
            m_ContextMenu.Clear();
            for (int i = 0; i < menu.Length; i++)
            {
                string menuItem = menu[i];
                m_ContextMenu.AddMenuItem(menuItem, delegate { Debug.Log("Used - " + menuItem); });
            }
            */
            m_ContextMenu.Show();
        }
    }
}
