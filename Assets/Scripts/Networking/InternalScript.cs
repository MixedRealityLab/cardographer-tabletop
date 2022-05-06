using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InternalScript : MonoBehaviour
{
    public GameObject localPlayer;
    public GameObject PlayerCustomisationPreview;

    //Player Customisation
    public Color PlayerColour = Color.grey;

    const string test = "testing1234";

    string session = "6272a237e2089a49f9d523c7";

    public string URL;
    public TMP_Text URLDisplay;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        URL = Application.absoluteURL;
        URLDisplay.text = "Room URL is: " + URL;
        
        //Debug.Log(new Guid("fe211431-6272-a237-e208-9a49f9d523c7"));
        //00000000-0000-0000-0000-000000000000
        //fe211431-6272-a237-e208-9a49f9d523c7
        //fe211431-6272-a237-e208-9a49f9d523c7
        //fe211431-6272-237e-089a-49f9d523c7
        //GUID salt is: fe211431
    }

    public void SwitchColour(int opt)
    {
        switch (opt)
        {
            case 1:
                PlayerColour = Color.red;
                break;
            case 2:
                PlayerColour = Color.green;
                break;
            case 3:
                PlayerColour = Color.blue;
                break;
            case 4:
                PlayerColour = Color.yellow;
                break;
            case 5:
                PlayerColour = Color.black;
                break;
            case 6:
                PlayerColour = Color.cyan;
                break;
            case 7:
                PlayerColour = Color.magenta;
                break;
            default:
                PlayerColour = Color.grey;
                break;
        }

        PlayerCustomisationPreview.GetComponent<Renderer>().material.color = PlayerColour;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
