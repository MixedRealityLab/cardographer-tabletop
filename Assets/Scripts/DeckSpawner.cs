using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TMPro;

[Serializable]
public class SaveObject
{
    public string[] Atlases;
    public SaveDeck[] Decks;
    public SaveCard[] Cards;
    public SaveBoard[] Boards;
}

[Serializable]
public struct SaveCard
{
    public int cardDeckID;
    public int cardGlobalID;
    public string cardName;
    public string cardDeckOrigin;
    public Vector3 cardPos;
    public Quaternion cardRot;
    public Vector3 cardSize;
    public SaveAnnotation[] cardAnnotations;
}

[Serializable]
public struct SaveDeck
{
    public string deckName;
    [SerializeField]
    public Card[] deckCards;
    public Vector3 deckPos;
    public Quaternion deckRot;
    public Vector3 deckCardSize;
    public SaveAnnotation[] deckAnnotations;
}

[Serializable]
public struct SaveBoard
{
    public string boardName;
    public string boardTitle;
    public string boardID;
    public string boardDeck;
    public string[] boardImageUrl;
    public Vector3 boardPos;
    public Quaternion boardRot;
    public int[] boardSize;
    public SaveAnnotation[] boardAnnotations;
}

[Serializable]
public struct SaveAnnotation
{
    public string annotationString;
    public Vector3 annotationPos;
    public Quaternion annotationRot;
}

public class DeckSpawner : NetworkBehaviour
{
    public GameObject DeckPrefabRework;
    public GameObject CardPrefabRework;
    public GameObject BoardPrefab;
    public GameObject AnnotationPrefab;

    //public Dropdown savedGames;
    public TMP_Dropdown savedGames;
    public string deckName;
    public string boardName;
    public Image tempload;

    public int selectedSave;

    List<string> saveGames = new List<string>();

    public AtlasConverter atlasConverter;

    List<string> availableCategories = new List<string>();
    List<string> availableBoards = new List<string>();

    NetworkRoom currentRoom;

    public ServerManagement playerServerManagement;

    private void Start()
    {
        tempload = GameObject.FindGameObjectWithTag("PreLoad").GetComponent<Image>();
        playerServerManagement = GameObject.FindGameObjectWithTag("ClientRelay").GetComponent<ServerManagement>();
    }

    public void populateDropdown()
    {
        availableCategories = atlasConverter.getCategiorySelection();
        TMP_Dropdown dropdown = GameObject.FindGameObjectWithTag("CatSelector").GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(availableCategories);
        deckName = availableCategories[0];
    }

    public void populateBoardDropdown()
    {
        availableBoards = atlasConverter.getBoardSelection();
        TMP_Dropdown dropdown = GameObject.FindGameObjectWithTag("CatBoardSelect").GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(availableBoards);
        boardName = availableBoards[0];
    }

    public void SpawnDeck()
    {
        Debug.Log("Deck to spawn: " + deckName);
        if (currentRoom == null) currentRoom = GameObject.FindGameObjectWithTag("RelayRoom").GetComponent<NetworkRoom>();
        if (!string.IsNullOrWhiteSpace(deckName)) CmdSpawnDeck(deckName, currentRoom.gameObject);
        else Debug.Log("No Deck Selected");
    }

    public void SpawnBoard()
    {
        Debug.Log("Board to spawn: " + boardName);
        if (currentRoom == null) currentRoom = GameObject.FindGameObjectWithTag("RelayRoom").GetComponent<NetworkRoom>();
        if (!string.IsNullOrWhiteSpace(boardName)) CmdSpawnBoard(boardName, currentRoom.gameObject);
        else Debug.Log("No Board Selected");
    }

    public void setSpawnDeck(int input)
    {
        Debug.Log("Option chosen: " + input);
        deckName = availableCategories[input];
        if (currentRoom == null) currentRoom = GameObject.FindGameObjectWithTag("RelayRoom").GetComponent<NetworkRoom>();
        currentRoom.CmdDownloadDeck(deckName);
    }

    public void setSpawnBoard(int input)
    {
        Debug.Log("Choosing board: " + input);
        boardName = availableBoards[input];
        if (currentRoom == null) currentRoom = GameObject.FindGameObjectWithTag("RelayRoom").GetComponent<NetworkRoom>();
    }

    [Command(requiresAuthority = false)]
    void CmdSpawnBoard(string name, GameObject localRoom)
    {
        Debug.Log("Server Spawn Board");
        GameObject temp = Instantiate(BoardPrefab);
        temp.transform.position = new Vector3(0, 2, 0);
        var Board = temp.GetComponent<BoardBase>();

        Board.BoardDetails = atlasConverter.getBoard(name);

        Debug.Log("Local Room: " + localRoom);
        temp.GetComponent<NetworkMatch>().matchId = localRoom.GetComponent<NetworkMatch>().matchId;
        NetworkServer.Spawn(temp);
    }

    [Command(requiresAuthority = false)]
    void CmdSpawnDeck(string name, GameObject localRoom)
    {
        Debug.Log("Server Spawn Deck");
        GameObject temp = Instantiate(DeckPrefabRework);
        temp.transform.position = new Vector3(0, 2, 0);
        var Deck = temp.GetComponent<DeckBase>();
        Deck.AtlasObject = atlasConverter;

        Card DeckCard = new Card();
        DeckCard.cardDeck = name;
        Deck.SetCardDetails(DeckCard);

        Debug.Log("Local Room: " + localRoom);
        temp.GetComponent<NetworkMatch>().matchId = localRoom.GetComponent<NetworkMatch>().matchId;
        NetworkServer.Spawn(temp);
    }

    //Saving State

    //Accessor Function for saving
    public void saveState()
    {
        CmdSaveState(GameObject.FindGameObjectWithTag("RelayRoom"), playerServerManagement.playerSession);
    }

    [Command(requiresAuthority = false)]
    public void CmdSaveState(GameObject relay, string playerSession)
    {
        Debug.Log(GameObject.FindGameObjectWithTag("RelayRoom").GetComponent<NetworkMatch>().matchId);
        saveTable(relay.GetComponent<NetworkMatch>().matchId, playerSession);
    }

    void saveTable(Guid currentRoom, string session)
    {
        //if (!Directory.Exists(Application.dataPath + "/SaveStates/"))
        if (!Directory.Exists("/app/data/" + session + "/SaveStates/"))
        {
            Debug.Log("SaveStates does not exist, creating");
            //Directory.CreateDirectory(Application.dataPath + "/SaveStates/");
            Directory.CreateDirectory("/app/data/" + session + "/SaveStates/");
        }
        //string file = Application.dataPath + "/SaveStates/" + DateTime.Now.ToFileTime() + ".json";
        string file = "/app/data/" + session + "/SaveStates/" + DateTime.UtcNow + ".json";
        Debug.Log(file);
        var save = new SaveObject();
        //List of Cards and atlases
        List<SaveCard> tempCards = new List<SaveCard>();
        List<string> tempAtlas = new List<string>();
        GameObject[] temp = GameObject.FindGameObjectsWithTag("Card");
        foreach (var item in temp)
        {
            if(item.GetComponent<CardBase>() != null)
            {
                if(item.GetComponent<NetworkMatch>().matchId == currentRoom)
                {
                    Debug.Log("Saving: " + item.GetComponent<CardBase>().cardDetails.cardName);
                    var card = new SaveCard();
                    card.cardDeckID = item.GetComponent<CardBase>().cardDetails.cardID;
                    card.cardDeckOrigin = item.GetComponent<CardBase>().cardDetails.cardDeck;
                    card.cardName = item.GetComponent<CardBase>().cardDetails.cardName;
                    card.cardGlobalID = item.GetComponent<CardBase>().cardDetails.globalID;
                    card.cardPos = item.transform.position;
                    card.cardRot = item.transform.rotation;
                    card.cardSize = item.GetComponent<CardBase>().cardDetails.cardSize;
                    card.cardAnnotations = GetAnnotations(item.GetComponentsInChildren<AnnotationBase>());
                    tempCards.Add(card);
                }
                
            }
            
        }

        List<SaveDeck> tempDecks = new List<SaveDeck>();
        GameObject[] searchedDecks = GameObject.FindGameObjectsWithTag("Deck");
        foreach (var item in searchedDecks)
        {
            if (item.GetComponent<DeckBase>() != null)
            {
                if (item.GetComponent<NetworkMatch>().matchId == currentRoom)
                {
                    Debug.Log("Saving deck base: " + item.GetComponent<DeckBase>().cardDetails.cardDeck);
                    var deck = new SaveDeck();
                    List<Card> cards = new List<Card>();

                    deck.deckName = item.GetComponent<DeckBase>().cardDetails.cardDeck;
                    deck.deckPos = item.transform.position;
                    deck.deckRot = item.transform.rotation;
                    deck.deckCardSize = item.GetComponent<DeckBase>().cardDetails.cardSize;
                    foreach (var cardItem in item.GetComponent<DeckBase>().CardList)
                    {
                        cards.Add(cardItem);
                        Debug.Log("Card Item for deck: " + cardItem.cardName);
                    }
                    deck.deckCards = cards.ToArray();
                    deck.deckAnnotations = GetAnnotations(item.GetComponentsInChildren<AnnotationBase>());
                    tempDecks.Add(deck);
                }
            }
                
        }

        List<SaveBoard> tempBoards = new List<SaveBoard>();
        GameObject[] searchedBoards = GameObject.FindGameObjectsWithTag("Board");
        foreach (var item in searchedBoards)
        {
            if (item.GetComponent<BoardBase>() != null)
            {
                if (item.GetComponent<NetworkMatch>().matchId == currentRoom)
                {
                    Debug.Log("Saving board base: " + item.GetComponent<BoardBase>().BoardDetails.boardName);
                    var board = new SaveBoard();
                    Board currBoard = item.GetComponent<BoardBase>().BoardDetails;
                    board.boardDeck = currBoard.boardDeck;
                    board.boardName = currBoard.boardName;
                    board.boardPos = item.transform.position;
                    board.boardRot = item.transform.rotation;
                    board.boardSize = currBoard.boardSize;
                    board.boardTitle = currBoard.boardTitle;
                    board.boardImageUrl = currBoard.boardImageURL;
                    board.boardID = currBoard.boardID;
                    board.boardAnnotations = GetAnnotations(item.GetComponentsInChildren<AnnotationBase>());
                    tempBoards.Add(board);
                }
            }
        }

        Debug.Log("Loop Finished");
        save.Cards = tempCards.ToArray();
        save.Atlases = tempAtlas.ToArray();
        save.Decks = tempDecks.ToArray();
        save.Boards = tempBoards.ToArray();
        var jsonSave = JsonUtility.ToJson(save);

        File.AppendAllText(file, jsonSave);
    }

    SaveAnnotation[] GetAnnotations(AnnotationBase[] annotations)
    {
        List<SaveAnnotation> tempAnnos = new List<SaveAnnotation>();
        foreach (var anno in annotations)
        {
            SaveAnnotation A = new SaveAnnotation();
            A.annotationString = anno.AnnotationText;
            A.annotationPos = anno.gameObject.transform.position;
            A.annotationRot = anno.gameObject.transform.rotation;
            tempAnnos.Add(A);
        }

        return tempAnnos.ToArray();
    }

    //Loading State

    public void getSaveGames()
    {
        Debug.Log("Is local player: " + NetworkClient.localPlayer.connectionToServer);
        Debug.Log(NetworkClient.localPlayer.gameObject);
        CmdGetSaveGames(NetworkClient.localPlayer, playerServerManagement.playerSession);
    }
  

    [Command(requiresAuthority = false)]
    void CmdGetSaveGames(NetworkIdentity client, string playerSession)
    {
        getSaves(playerSession);
        TargetUpdateSavedGameDropdown(client.connectionToClient, saveGames);
    }

    [TargetRpc]
    void TargetUpdateSavedGameDropdown(NetworkConnection target, List<string> games)
    {
        savedGames.ClearOptions();
        savedGames.AddOptions(games);
    }

    void getSaves(string session)
    {
        //var info = new DirectoryInfo(Application.dataPath + "/SaveStates/");
        var info = new DirectoryInfo("/app/data/" + session + "/SaveStates/");
        var fileInfo = info.GetFiles();
        Regex rx = new Regex("meta");
        Match mx;
        saveGames.Clear();
        foreach (var file in fileInfo)
        {
            mx = rx.Match(file.Name);
            if (mx != Match.Empty)
            {
                Debug.Log("Meta file, ignoring");
            }
            else
            {
                saveGames.Add(file.Name);
                Debug.Log(file.Name);
            }
        }
    }

    public void setSelectedSave(int sel)
    {
        selectedSave = sel;
    }

    public void loadState()
    {
        CmdLoadState(selectedSave, GameObject.FindGameObjectWithTag("RelayRoom"), playerServerManagement.playerSession);
    }

    [Command(requiresAuthority = false)]
    public void CmdLoadState(int option, GameObject relay, string playerSession)
    {
        loadTable(option, relay, playerSession);
    }

    SaveObject loadedSave;

    void loadTable(int option, GameObject relay, string session)
    {
        Debug.Log("Game to load: " + saveGames[option]);
        //string temp = File.ReadAllText(Application.dataPath + "/SaveStates/" + saveGames[option]);
        string temp = File.ReadAllText("/app/data/" + session + "/SaveStates/" + saveGames[option]);
        Debug.Log(temp);
        loadedSave = JsonConvert.DeserializeObject<SaveObject>(temp);
        Debug.Log("Loaded card count: " + loadedSave.Cards.Length);
        Debug.Log("Loaded deck count: " + loadedSave.Decks.Length);
        foreach (var item in loadedSave.Cards)
        {
            Card tempCard = new Card();
            tempCard.cardDeck = item.cardDeckOrigin;
            tempCard.cardID = item.cardDeckID;
            tempCard.cardName = item.cardName;
            tempCard.globalID = item.cardGlobalID;
            tempCard.cardSize = item.cardSize;
            spawnCardFromSave(tempCard, relay, item.cardPos, item.cardRot, item.cardAnnotations);
        }
        foreach (var itemDeck in loadedSave.Decks)
        {
            spawnDeckFromSave(itemDeck.deckName, itemDeck.deckCardSize, relay, itemDeck.deckPos, itemDeck.deckRot, itemDeck.deckCards, itemDeck.deckAnnotations);
        }
        foreach (var item in loadedSave.Boards)
        {
            spawnBoardFromSave(item, relay, item.boardAnnotations);
        }
    }


    [Server]
    void spawnCardFromSave(Card details, GameObject relay, Vector3 pos, Quaternion rot, SaveAnnotation[] annotationArray)
    {
        Debug.Log("Spawning Card: " + details.cardName);
        GameObject temp = Instantiate(CardPrefabRework);
        temp.GetComponent<CardBase>().SetCardDetails(details);
        //relay.GetComponent<NetworkRoom>().cards.Add(temp);
        if(details.globalID > relay.GetComponent<NetworkRoom>().cardCount) relay.GetComponent<NetworkRoom>().cardCount = details.globalID;
        temp.GetComponent<NetworkMatch>().matchId = relay.GetComponent<NetworkMatch>().matchId;
        temp.transform.position = new Vector3(pos.x, 2, pos.z);
        temp.transform.rotation = rot;
        NetworkServer.Spawn(temp);
        foreach (var item in annotationArray)
        {
            spawnAnnotationFromSave(item, temp, relay);
        }
        
    }

    [Server]
    void spawnDeckFromSave(string name, Vector3 deckSize, GameObject relay, Vector3 pos, Quaternion rot, Card[] cards, SaveAnnotation[] annotationArray)
    {
        Debug.Log("Spawning Deck: " + name);
        GameObject temp = Instantiate(DeckPrefabRework);
        
        var Deck = temp.GetComponent<DeckBase>();
        if (atlasConverter == null) atlasConverter = GameObject.FindGameObjectWithTag("AtlasCon").GetComponent<AtlasConverter>();
        Deck.AtlasObject = atlasConverter;
        foreach (var item in cards)
        {
            Deck.GetComponent<DeckBase>().AddCardToDeck(item);
        }
        Card DeckCard = new Card();
        DeckCard.cardDeck = name;
        DeckCard.cardSize = deckSize;
        Deck.SetCardDetails(DeckCard);

        temp.GetComponent<NetworkMatch>().matchId = relay.GetComponent<NetworkMatch>().matchId;

        temp.transform.position = new Vector3(pos.x, 2, pos.z);
        temp.transform.rotation = rot;
        NetworkServer.Spawn(temp);
        foreach (var item in annotationArray)
        {
            spawnAnnotationFromSave(item, temp, relay);
        }
    }

    [Server]
    void spawnBoardFromSave(SaveBoard board, GameObject relay, SaveAnnotation[] annotationArray)
    {
        Debug.Log("Spawning Deck: " + name);
        GameObject temp = Instantiate(BoardPrefab);

        var Board = temp.GetComponent<BoardBase>();
        if (atlasConverter == null) atlasConverter = GameObject.FindGameObjectWithTag("AtlasCon").GetComponent<AtlasConverter>();

        Board currBoard = new Board();
        currBoard.boardDeck= board.boardDeck;
        currBoard.boardName = board.boardName;
        currBoard.boardSize = board.boardSize;
        currBoard.boardTitle = board.boardTitle;
        currBoard.boardImageURL = board.boardImageUrl;
        currBoard.boardID = board.boardID;

        Board.BoardDetails = currBoard;

        temp.GetComponent<NetworkMatch>().matchId = relay.GetComponent<NetworkMatch>().matchId;

        temp.transform.position = new Vector3(board.boardPos.x, 1.5f, board.boardPos.z);
        temp.transform.rotation = board.boardRot;
        NetworkServer.Spawn(temp);
        foreach (var item in annotationArray)
        {
            spawnAnnotationFromSave(item, temp, relay);
        }
    }

    [Server]
    void spawnAnnotationFromSave(SaveAnnotation anno, GameObject parent, GameObject relay)
    {
        Debug.Log("Spawning Annotation: " + anno.annotationString);
        GameObject temp = Instantiate(AnnotationPrefab);
        temp.GetComponent<NetworkMatch>().matchId = relay.GetComponent<NetworkMatch>().matchId;
        temp.GetComponent<AnnotationBase>().AnnotationText = anno.annotationString;
        temp.GetComponent<AnnotationBase>().ParentObject = parent;
        temp.transform.position = anno.annotationPos;
        temp.transform.rotation = anno.annotationRot;
        temp.transform.SetParent(parent.transform);
        NetworkServer.Spawn(temp);
    }
}