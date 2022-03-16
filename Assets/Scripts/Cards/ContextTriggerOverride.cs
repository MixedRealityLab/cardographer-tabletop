using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.UIWidgets;
using ContextMenu = DevionGames.UIWidgets.ContextMenu;
using UnityEngine.EventSystems;

public abstract class ContextTriggerOverride : MonoBehaviour, IPointerDownHandler
{
    public abstract void OnPointerDown(PointerEventData eventData);
}

/*
private ContextMenu m_ContextMenu;

    //public string[] menu;

    // Start is called before the first frame update
    private void Start()
    {
        m_ContextMenu = WidgetUtility.Find<ContextMenu>("ContextMenu");
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Clicky");
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            m_ContextMenu.Clear();
            for (int i = 0; i < menu.Length; i++)
            {
                string menuItem = menu[i];
                m_ContextMenu.AddMenuItem(menuItem, delegate { Debug.Log("Used - " + menuItem); });
            }
            
m_ContextMenu.Show();
        }
    } 
*/