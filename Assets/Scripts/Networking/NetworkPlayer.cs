using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject internals;
    public ServerManagement clientRelay;
    [Command]
    void CmdUpdatePos(Vector3 pos)
    {
        transform.position = pos;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            transform.position = GameObject.FindGameObjectWithTag("MainCamera").transform.position;
            internals = GameObject.FindGameObjectWithTag("Internals");
            internals.GetComponent<InternalScript>().localPlayer = gameObject;

            clientRelay = GameObject.FindGameObjectWithTag("ClientRelay").GetComponent<ServerManagement>();
            clientRelay.askRoomList();
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
