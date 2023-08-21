using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.IO;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

[System.Serializable]
public struct Room
{
    public string roomName;
    public int roomPlayers;
    public Guid roomRef;
}

public class ServerManagement : NetworkBehaviour
{
    public List<Room> roomList;
    public Guid testGuid;
    public GameObject LobbyCanvas;
    public GameObject ListViewport;
    public GameObject ListButtonPrefab;

    public List<GameObject> relayList;

    public GameObject roomRelayPrefab;
    public GameObject deckSpawnerPrefab;

    public GameObject roomRelay;
    public GameObject localPlayer;
    public GameObject deckSpawner;

    public UIController UiControl;

    public TMP_Text tempText;
    public InternalScript internals;

    public AtlasConverter atlasConverter;
    public string playerSession; //Client only
    const string guidSalt = "fe211431-";

    DeckSpawner roomDeckSpawner;

    //Network Calls
    [Server]
    Guid generateGuid(string session)
    {
        return new Guid(guidSalt + session.Substring(0, 4) + "-" + session.Substring(4, 4) + "-" + session.Substring(8, 4) + "-" + session.Substring(12));
    }

    [Command(requiresAuthority = false)]
    public void CmdAskJoin(NetworkIdentity client, GameObject player, string incomingRoomName)
    {
        Guid roomID = roomList.Find(x => x.roomName == incomingRoomName).roomRef;
        player.GetComponent<NetworkMatch>().matchId = roomID;
        TargetAskJoin( client.connectionToClient );
    }

    [Command(requiresAuthority = false)]
    void CmdJoinSession(NetworkIdentity client, GameObject player, string session)
    {
        player.GetComponent<NetworkMatch>().matchId = generateGuid(session);
        TargetAskJoin(client.connectionToClient);
    }

    [TargetRpc]
    public void TargetAskJoin(NetworkConnection conn)
    {
        if(roomDeckSpawner == null) roomDeckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner").GetComponent<DeckSpawner>();
        UiControl = GameObject.FindGameObjectWithTag("UIController").GetComponent<UIController>();
        roomDeckSpawner.populateDropdown();
        roomDeckSpawner.populateBoardDropdown();
        UiControl.ToggleOptionsMenu();
        if (roomRelay == null) roomRelay = GameObject.FindGameObjectWithTag("RelayRoom");
        LobbyCanvas.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    void CmdCreateSessionRoom(string session)
    {
        GameObject relay = Instantiate(roomRelayPrefab);
        relay.GetComponent<NetworkMatch>().matchId = generateGuid(session);
        relayList.Add(relay);
        NetworkServer.Spawn(relay);
    }

    [Client]
    void GetDeckInfo()
    {
        //Get deck info for specific session
        Debug.Log("Checking Session");
        Regex rx = new Regex(@"sessions\/(.+)\/tabletop");
        //string test = "https://cardographer.cs.nott.ac.uk/platform/user/sessions/6272a237e2089a49f9d523c7/tabletop";
        //string test = "https://cardographer.cs.nott.ac.uk/platform/user/sessions/627a466bedae149dc742e1ee/tabletop";
        //string test = "https://cardographer.cs.nott.ac.uk/platform/user/sessions/6267f29fd882d37e56cca690/tabletop";
        //string test = "https://cardographer.cs.nott.ac.uk/platform/user/sessions/625573821d877952c3463d29/tabletop";
        //string test = "https://cardographer.cs.nott.ac.uk/platform/user/sessions/627a466bedae149dc742e1ee/tabletop";
        string test = "https://cardographer.cs.nott.ac.uk/sessions/627a466bedae149dc742e1ee/tabletop";

        //string test = internals.URL;

        MatchCollection matches = rx.Matches(test);
        Debug.Log("Match count: " + matches.Count);
        foreach (Match item in matches)
        {
            Debug.Log(item.Groups[1]);
            CmdGetDeckInfo(NetworkClient.localPlayer.gameObject ,item.Groups[1].ToString());
            //CmdGetDeckInfo(NetworkClient.localPlayer ,item.Groups[1].ToString());
        }

    }

    [Command(requiresAuthority = false)]
    void CmdGetDeckInfo(GameObject player ,string session)
    {
        Debug.Log("SessionID is: " + session);
        Debug.Log("DeckInfo text is: " + File.ReadAllText("/app/data/sessions/" + session + "/DeckInfo.json") );
        if (atlasConverter == null) atlasConverter = GameObject.FindGameObjectWithTag("AtlasCon").GetComponent<AtlasConverter>();
        atlasConverter.AccessDeckInfo(File.ReadAllText("/app/data/sessions/" + session + "/DeckInfo.json"));
        TargetGetDeckInfo(player.GetComponent<NetworkIdentity>().connectionToClient, File.ReadAllText("/app/data/sessions/" + session + "/DeckInfo.json"), session);
    }

    [TargetRpc]
    void TargetGetDeckInfo(NetworkConnection player, string deckInfo, string session)
    {
        Debug.Log(deckInfo);
        tempText.text = deckInfo;
        if (atlasConverter == null) atlasConverter = GameObject.FindGameObjectWithTag("AtlasCon").GetComponent<AtlasConverter>();
        atlasConverter.AccessDeckInfo(deckInfo);
        playerSession = session;
    }

    //Network Calls

    //Client Methods
    [Client]
    void ListRegenerate()
    {
        foreach (var item in roomList)
        {
            ListAddItem(item);
        }
    }

    [Client]
    void ListAddItem(Room roomData)
    {
        GameObject temp = Instantiate(ListButtonPrefab);
        temp.transform.SetParent(ListViewport.transform, true);
        temp.GetComponentInChildren<Text>().text = roomData.roomName;
        temp.GetComponent<Button>().onClick.AddListener(() => ListButtonJoin(roomData.roomName));
    }

    

    [Client]
    void ListButtonJoin(string id)
    {
        Debug.Log("Button clicked, id: " + id);
        if (localPlayer == null) localPlayer = GetComponentInParent<InternalScript>().localPlayer;
        CmdAskJoin(localPlayer.GetComponent<NetworkIdentity>(), localPlayer, id);
    }

    [Client]
    void clientSetup()
    {
        Debug.Log("Client Setup");
        roomRelay = GameObject.FindGameObjectWithTag("RelayRoom");
        LobbyCanvas = GameObject.FindGameObjectWithTag("LobbyScreen");
        deckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner");
        localPlayer = GetComponentInParent<InternalScript>().localPlayer;
        GetDeckInfo();
    }

    [Client]
    public void joinRoom()
    {
        if (playerSession != "")
        {
            CmdCreateSessionRoom(playerSession);
            Debug.Log("Joining session: " + playerSession);
            if (localPlayer == null) localPlayer = GetComponentInParent<InternalScript>().localPlayer;
            CmdJoinSession(localPlayer.GetComponent<NetworkIdentity>(), localPlayer, playerSession);
        }
    }
    //Client Methods

    void Start()
    {
        Debug.Log("Server management method");
        //roomList = new List<Room>();
        relayList = new List<GameObject>();
        roomDeckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner").GetComponent<DeckSpawner>();
        clientSetup();        
    }

}



/*
 * Old Room list generation code
 * Code to create, list, and join custom rooms 
    [Command(requiresAuthority = false)]
    void CmdCreateRoom()
    {
        Debug.Log("Creating Room");
        Guid roomGuid = Guid.NewGuid();
        GameObject relay = Instantiate(roomRelayPrefab);
        //relay.GetComponent<NetworkMatchChecker>().matchId = roomGuid;
        relay.GetComponent<NetworkMatch>().matchId = roomGuid;
        relayList.Add(relay);
        NetworkServer.Spawn(relay);

        Room newRoomInfo = new Room();
        newRoomInfo.roomName = "" + (roomList.Count + 1);
        newRoomInfo.roomPlayers = 0;
        newRoomInfo.roomRef = roomGuid;
        roomList.Add(newRoomInfo);
        RpcUpdateClientLists(newRoomInfo);
    }

    [ClientRpc]
    void RpcUpdateClientLists(Room roomData)
    {
        roomList.Add(roomData);
        ListAddItem(roomData);
    }

    [Command(requiresAuthority = false)]
    void CmdAskRoomList(NetworkIdentity client)
    {
        TargetSendRoomList(client.connectionToClient, roomList);
    }

    [TargetRpc]
    void TargetSendRoomList(NetworkConnection conn, List<Room> incomingList)
    {
        roomList = incomingList;
        ListRegenerate();
    }

    [Client]
    public void askRoomList()
    {
        if (localPlayer == null) localPlayer = GetComponentInParent<InternalScript>().localPlayer;
        CmdAskRoomList(localPlayer.GetComponent<NetworkIdentity>());
    }

    [Client]
    public void createRoom()
    {
        CmdCreateRoom();
    }

    */
