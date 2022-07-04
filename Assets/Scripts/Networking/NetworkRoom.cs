using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

[Serializable]
public struct DeckStored
{
    public string name;
    public List<CardTextures> cardTex;
}

public class NetworkRoom : NetworkBehaviour
{
    public List<DeckStored> storedDecks = new List<DeckStored>();
    
    //public List<CardTextures> cardTexList = new List<CardTextures>();
    public readonly SyncList<string> cardDeckList = new SyncList<string>();

    public readonly SyncList<GameObject> CardsInUse = new SyncList<GameObject>();

    public int cardCount = 0;
    public List<GameObject> cards;

    public AtlasConverter atlasConverter;
    public DeckSpawner deckSpawner;

    //Network Commands

    [Command(requiresAuthority = false)]
    public void CmdDownloadDeck(string name)
    {
        if(cardDeckList.Find(x => x == name) == null ) cardDeckList.Add(name);
    }
    //Network Commands
    //Server methods
    [Server]
    void CheckForSavedGames()
    {
        Debug.Log("Checking for auto saves and then loading");
        if (deckSpawner == null) deckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner").GetComponent<DeckSpawner>();
        deckSpawner.ServerAutoLoad(gameObject, extractSession(GetComponent<NetworkMatch>().matchId));
    }

    [Server]
    void CheckActiveUsers()
    {
        if (deckSpawner == null) deckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner").GetComponent<DeckSpawner>();
        InvokeRepeating("ActiveSearch", 300f, 300f);
    }

    [Server]
    string extractSession(Guid netGuid)
    {
        return netGuid.ToString().Substring(9, 4) + netGuid.ToString().Substring(14, 4) + netGuid.ToString().Substring(19, 4) + netGuid.ToString().Substring(24);
    }

    void ActiveSearch()
    {
        int playerCount = 0;
        foreach (var player in GameObject.FindGameObjectsWithTag("Players"))
        {
            Debug.Log("Active search start");
            if(player.GetComponent<NetworkMatch>().matchId == GetComponent<NetworkMatch>().matchId) 
                playerCount++;
        }
        if (playerCount <= 0) SaveAndDeleteRoom();
    }

    void SaveAndDeleteRoom()
    {
        Debug.Log("Deleting Room");
        CancelInvoke();
        if(deckSpawner == null) deckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner").GetComponent<DeckSpawner>();
        deckSpawner.saveTableServer(GetComponent<NetworkMatch>().matchId, extractSession(GetComponent<NetworkMatch>().matchId), true);

    }
    //Server methods
    //Client methods
    [Client]
    public void storeDeck(CardTextures cardTex)
    {
        Debug.Log("Storing Deck");
        Debug.Log(cardTex.deckName);
        Debug.Log(storedDecks.Exists(x => x.name == cardTex.deckName));
        //Check if deck is already stored
        if(storedDecks.Exists(x => x.name == cardTex.deckName))
        {
            Debug.Log("Deck Found, checking for card tex");
            DeckStored tempDeck = storedDecks.Find(x => x.name == cardTex.deckName);
            //Next check if current texture is already stored in the deck
            //If not, add card texture to the struct
            if (!tempDeck.cardTex.Exists(x => x.cardID == cardTex.cardID))
            {
                Debug.Log("Tex not found, adding");
                tempDeck.cardTex.Add(cardTex);
                return;
            }
        }
        else
        {
            //If no deck present, initalise new struct
            DeckStored newDeck = new DeckStored();
            newDeck.name = cardTex.deckName;
            newDeck.cardTex = new List<CardTextures>();
            newDeck.cardTex.Add(cardTex);
            storedDecks.Add(newDeck);

        }
    }

    [Client]
    public Texture2D getCardTexture(string deckName, int cardID, bool cardBack)
    {
        //Debug.Log("Origin Deck: " + deckName);
        //Debug.Log("Card ID: " + cardID);
        //Debug.Log("Card Back: " + cardBack);
        DeckStored foundDeck = storedDecks.Find(x => x.name == deckName);
        //Debug.Log("Found Deck: " + foundDeck.name);
        if (string.IsNullOrEmpty(foundDeck.name)) return null;
        CardTextures foundCardTextures = foundDeck.cardTex.Find(x => x.cardID == cardID);

        Texture2D foundTex = cardBack ? foundCardTextures.textureBack: foundCardTextures.textureFront;
        return foundTex;
    }

    [Client]
    IEnumerator getAtlasConverter()
    {
        atlasConverter = GameObject.FindGameObjectWithTag("AtlasCon").GetComponent<AtlasConverter>();
        Debug.Log("Card Deck List count in Relay Room: " + cardDeckList.Count);
        foreach (var item in cardDeckList)
        {
            Debug.Log("Preloading item: " + item);
            if (!string.IsNullOrEmpty(item))
            {
                yield return StartCoroutine(atlasConverter.GetComponent<AtlasConverter>().AtlasDownload(item));
                yield return atlasConverter.GetComponent<AtlasConverter>().atlasRef.Exists(x => x.deckName == item);
                DeckInfo newDeck = atlasConverter.GetComponent<AtlasConverter>().getDeck(item);
                for (int i = 0; i < newDeck.cardCount - 1; i++)
                {
                    generateTexture(i, newDeck);
                }
            }
        }
    }

    [Client]
    void generateTexture(int currentTex, DeckInfo generateDeck)
    {
        Debug.Log("Generate Texture " + currentTex + "/" + generateDeck.cardCount + " for " + generateDeck.deckName);
        CardTextures cardTex = new CardTextures();
        cardTex.cardID = currentTex;
        cardTex.textureFront = generateDeck.cardTextures[currentTex];
        cardTex.textureBack = generateDeck.cardTextures.Last();
        cardTex.deckName = generateDeck.deckName;
        storeDeck(cardTex);
    }

    //Client methods

    void Start()
    {
#if !UNITY_SERVER
        StartCoroutine(getAtlasConverter());
#endif
        CheckForSavedGames();
        CheckActiveUsers();
    }

    public int getGlobalIndex()
    {
        int temp = cardCount;
        cardCount++;
        return temp;
    }

    //Revamped methods
    public void DownloadRef(string name)
    {
        //DeckInfo temp = atlasConverter.GetComponent<AtlasConverter>().getDeck(name);
        StartCoroutine(atlasConverter.GetComponent<AtlasConverter>().AtlasDownload(name));
        Debug.Log("Downloading deck ref for: " + name);
        //Debug.Log(temp);
    }

    public void DeckFromRef(string name)
    {
        DeckInfo temp = atlasConverter.GetComponent<AtlasConverter>().getDeck(name);
        for (int i = 0; i < temp.cardCount - 1; i++)
        {
            generateTexture(i, temp);
        }
    }

    public bool IsRefCachec(string name)
    {
        NamedAtlas tempAtlas = atlasConverter.GetComponent<AtlasConverter>().atlasRef.Find(x => x.deckName == name);
        return tempAtlas.deckAtlas.Count > 0 ? true : false;
    }

    [Server]
    public void AddCardToAcvtiveList(GameObject card)
    {
        CardsInUse.Add(card);
    }
}
