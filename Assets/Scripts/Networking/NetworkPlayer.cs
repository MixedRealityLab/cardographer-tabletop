using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject internals;
    public ServerManagement clientRelay;
    public GameObject CursorObj;

    [SyncVar]
    public Color PlayerColour = Color.grey;

    [Command]
    void CmdUpdatePos(Vector3 pos)
    {
        transform.position = pos;
    }
    
    [Command]
    void CmdUpdatePlayerCustomisation(Color col)
    {
        PlayerColour = col;
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            transform.position = GameObject.FindGameObjectWithTag("MainCamera").transform.position;
            internals = GameObject.FindGameObjectWithTag("Internals");
            internals.GetComponent<InternalScript>().localPlayer = gameObject;
            CmdUpdatePlayerCustomisation(internals.GetComponent<InternalScript>().PlayerColour);
            CursorObj.GetComponent<Renderer>().material.color = internals.GetComponent<InternalScript>().PlayerColour;
            clientRelay = GameObject.FindGameObjectWithTag("ClientRelay").GetComponent<ServerManagement>();
            clientRelay.askRoomList();
        }
        else
        {
            CursorObj.GetComponent<Renderer>().material.color = PlayerColour;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            //CmdUpdatePos(GameObject.FindGameObjectWithTag("MainCamera").transform.position);
        }
    }
}
