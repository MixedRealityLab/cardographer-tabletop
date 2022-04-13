using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InternalScript : MonoBehaviour
{
    public GameObject localPlayer;
    public GameObject PlayerCustomisationPreview;

    //Player Customisation
    public Color PlayerColour = Color.grey;

    public string URL;
    public TMP_Text URLDisplay;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        URL = Application.absoluteURL;
        URLDisplay.text = "Room URL is: " + URL;
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
