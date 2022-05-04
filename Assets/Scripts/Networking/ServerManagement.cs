﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.IO;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using TMPro;

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

    DeckSpawner roomDeckSpawner;

    //Network Calls
    [Command(requiresAuthority = false)]
    public void CmdAskJoin(NetworkIdentity client, GameObject player, string incomingRoomName)
    {
        Guid roomID = roomList.Find(x => x.roomName == incomingRoomName).roomRef;
        //player.GetComponent<NetworkMatchChecker>().matchId = roomID;
        player.GetComponent<NetworkMatch>().matchId = roomID;
        TargetAskJoin( client.connectionToClient );
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
    void GetDeckInfo()
    {
        Debug.Log("Checking Session");
        Regex rx = new Regex(@"sessions\/(.+)\/tabletop");
        string test = "https://cardographer.cs.nott.ac.uk/platform/user/sessions/625573821d877952c3463d29/tabletop";

        //string test = internals.URL;

        MatchCollection matches = rx.Matches(test);
        Debug.Log("Match count: " + matches.Count);
        foreach (Match item in matches)
        {
            Debug.Log(item.Groups[1]);
            CmdGetDeckInfo(NetworkClient.localPlayer ,item.Groups[1].ToString());
        }

    }

    [Command(requiresAuthority = false)]
    void CmdGetDeckInfo(NetworkIdentity player ,string session)
    {
        Debug.Log("SessionID is: " + session);
        Debug.Log("DeckInfo text is: " + File.ReadAllText("/app/data/sessions/" + session + "/DeckInfo.json") );
        TargetGetDeckInfo(player.connectionToServer, File.ReadAllText("/app/data/sessions/" + session + "/DeckInfo.json"));
    }

    [TargetRpc]
    void TargetGetDeckInfo(NetworkConnection player, string boards)
    {
        Debug.Log(boards);
        tempText.text = boards;
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
    public void askRoomList()
    {
        if (localPlayer == null) localPlayer = GetComponentInParent<InternalScript>().localPlayer;
        CmdAskRoomList(localPlayer.GetComponent<NetworkIdentity>());
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
        
        roomRelay = GameObject.FindGameObjectWithTag("RelayRoom");
        LobbyCanvas = GameObject.FindGameObjectWithTag("LobbyScreen");
        deckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner");
        localPlayer = GetComponentInParent<InternalScript>().localPlayer;
        GetDeckInfo();
    }

    [Client]
    public void createRoom()
    {
        CmdCreateRoom();
    }
    //Client Methods

    // Start is called before the first frame update
    void Start()
    {
        roomList = new List<Room>();
        relayList = new List<GameObject>();
        roomDeckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner").GetComponent<DeckSpawner>();
        clientSetup();

        
    }

}
