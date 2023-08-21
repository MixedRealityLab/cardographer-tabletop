using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkManager))]
public class CustomNetworkHUD : MonoBehaviour
{
    NetworkManager manager;

    private void Start()
    {
    }

    void Awake()
    {
        manager = GetComponent<NetworkManager>();
    }

    public void joinServer()
    {
        manager.StartClient();
        Debug.Log(manager.networkAddress);
        //manager.networkAddress = "90.251.42.122";
        /*if ( string.IsNullOrWhiteSpace(""))
        {
            //Add popup for users to say that no username is entered etc....
            Debug.Log("No username");
        }
        else
        {
            
        }*/
    }

    public void leaveServer()
    {
        manager.StopClient();
    }

    public void startServer()
    {
        manager.StartServer();
    }

    public void stopServer()
    {
        manager.StopServer();
    }

}

