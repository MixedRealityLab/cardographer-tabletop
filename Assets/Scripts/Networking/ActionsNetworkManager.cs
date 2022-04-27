using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror.SimpleWeb;

public class ActionsNetworkManager : NetworkManager
{
    private NetworkConnection connection;

    public Guid ServerGuid;

    public override void Start()
    {
        Debug.Log("Start on Net man");
        var web = GetComponent<SimpleWebTransport>();
        if (networkAddress == "cardographer.cs.nott.ac.uk")
        {
            web.port = 443;
        }
#if UNITY_SERVER
        web.port = 7778;
#endif
        web.sslEnabled = web.port == 443;
        base.Start();
    }

    [ServerCallback]
    public Guid getServerGuid()
    {
        return ServerGuid;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //Override methods
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("Player Gone Server");

        
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Player Gone Client");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerGuid = Guid.NewGuid();
        Debug.Log("Server Start: " + ServerGuid);
        Debug.Log(transport.ServerUri());
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
    }
}
