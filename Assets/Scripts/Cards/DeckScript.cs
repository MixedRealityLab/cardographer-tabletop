using DevionGames.UIWidgets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class DeckScript : CardBehaviour
{
    [SyncVar]
    public int deckSize = 0;

    public List<Card> cardList = new List<Card>();

    public GameObject drawnCard;
    public GameObject CardPrefab;
    public GameObject FakeCardPrefab;
    [SyncVar]
    public bool spawned = false;
    public bool cardSpawned = false;
    [SyncVar]
    public bool deckDrag = false;
    [SyncVar]
    public bool cardDraw = false;

    [SyncVar]
    public Vector3 deckCardSize;
    //public CardPool cardPool;

    //Deck Name, only used for spawning decks
    [SyncVar]
    public string deckName;

    public AtlasConverter atlasConverter;

    private float lastClick = 0;
    private float waitTime = 0.5f; //wait time befor reacting
    private float downTime; //internal time from when the key is pressed
    private bool isHandled = false;
    private bool localHeld = false;
    private DeckInfo currentDeck;

    //Network Commands
    [Command(requiresAuthority = false)]
    void CmdSyncDeck(NetworkIdentity client)
    {
        TargetSyncDeck(client.connectionToClient, cardList);
    }

    [TargetRpc]
    void TargetSyncDeck(NetworkConnection client, List<Card> cards)
    {
        Debug.Log("Syncing cardlists");
        cardList = cards;
        foreach (var item in cardList)
        {
            Debug.Log("Syncing card: " + item);
            spawnFalseCard(item);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSetActiveCard(int id, Vector3 mousePos, NetworkIdentity client)
    {
        
        //Debug.Log("Card Count in pool: " + cardPool.cards.Count);
        Debug.Log("Relay Card Count: " + relayRoom.GetComponent<NetworkRoom>().cards.Count);
        Debug.Log("Card Global ID: " + id);
        //deckSize--;
        //GameObject temp = cardPool.cardSync.Find(x => x.GetComponent<CardBehaviour>().getCardDetails().globalID == id);
        //GameObject temp = cardPool.cards.Find(x => x.GetComponent<CardBehaviour>().getCardDetails().globalID == id);
        GameObject temp = relayRoom.GetComponent<NetworkRoom>().cards.Find(x => x.GetComponent<CardBehaviour>().getCardDetails().globalID == id);
        temp.SetActive(true);
        //temp.transform.position = mousePos;
        NetworkServer.Spawn(temp);
        RpcActivateCard(temp);
        TargetDrawCard(client.connectionToClient, mousePos , temp);
        EditDeck();
        RpcEditDeck();
    }

    [TargetRpc]
    void TargetDrawCard(NetworkConnection client, Vector3 mousePos, GameObject card)
    {
        card.transform.position = mousePos;
        card.GetComponent<CardBehaviour>().cardDragStart();
        card.GetComponent<CardBehaviour>().isCardHeld = true;
        card.GetComponent<CardBehaviour>().createDeck = false;
        drawnCard = card;
    }

    [ClientRpc]
    void RpcActivateCard(GameObject card)
    {
        card.SetActive(true);
        card.transform.localScale = atlasConverter.ResizeCard(card.GetComponent<CardBehaviour>().cardSize);
        //deckSize--;
    }

    [Server]
    void CmdSpawnCard(Card details)
    {
        Debug.Log("Spawning Card: " + details.cardName);
        GameObject temp = Instantiate(CardPrefab);
        temp.GetComponent<CardBehaviour>().SetCardDetails(details);
        Debug.Log(details.cardID);
        relayRoom.GetComponent<NetworkRoom>().cards.Add(temp);

        temp.GetComponent<BoxCollider>().enabled = false;
        //temp.GetComponent<NetworkMatchChecker>().matchId = GetComponent<NetworkMatchChecker>().matchId;
        temp.GetComponent<NetworkMatch>().matchId = GetComponent<NetworkMatch>().matchId;
        temp.SetActive(false);
        RpcAdjustFalseCard();
    }


    [Server]
    void CmdAddCard(Card details)
    {
        Debug.Log("Deck size CMD add card" + deckSize);
        cardList.Add(details);
        deckSize++;
        RpcAddCardList(details);
        spawnFalseCard(details);
        RpcSpawnFalseCard(details);
        

    }

    [ClientRpc]
    void RpcAdjustFalseCard()
    {
        Debug.Log("Deck size RPC adjust false" + deckSize);
        adjustFalseCard();
    }
    [ClientRpc]
    void RpcSpawnFalseCard(Card details)
    {
        Debug.Log("Deck size RPC spawn false" + deckSize);
        spawnFalseCard(details);
    }
    [ClientRpc]
    void RpcAddCardList(Card details)
    {
        Debug.Log("Deck size RPC add card" + deckSize);
        cardList.Add(details);
        deckSize++;
    }
    [ClientRpc]
    void RpcEditDeck()
    {
        EditDeck();
    }

    [ClientRpc]
    void RpcMergeDecks(int size, List<Card> deckCardList)
    {
        Debug.Log("RPC Merging decks");
        deckSize += size;
        Debug.Log(deckCardList.Count);
        foreach (var item in deckCardList)
        {
            cardList.Add(item);
            spawnFalseCard(item);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdDeleteDeck()
    {
        Destroy(gameObject);
        RpcDeleteDeck();
    }

    [ClientRpc]
    void RpcDeleteDeck()
    {
        Destroy(gameObject);
    }

    //Network Commands

    [Client]
    void generateTexture(int currentTex)
    {
        Debug.Log("Generate Texture " + currentTex + "/" + currentDeck.cardCount + " for " + deckName);
        CardTextures cardTex = new CardTextures();
        Debug.Log(currentDeck.cardTextures.Count);
        cardTex.cardID = currentTex;
        cardTex.textureFront = currentDeck.cardTextures[currentTex];
        cardTex.textureBack = currentDeck.cardTextures.Last();
        cardTex.deckName = deckName;
        if (relayRoom == null) relayRoom = GameObject.FindGameObjectWithTag("RelayRoom");
        //relayRoom.GetComponent<NetworkRoom>().cardTexList.Add(cardTex);
        relayRoom.GetComponent<NetworkRoom>().storeDeck(cardTex);
    }

    public IEnumerator GenerateDeck()
    {
        Debug.Log("Generating deck for: " + deckName);
        if (!string.IsNullOrEmpty(deckName))
        {
            yield return StartCoroutine(atlasConverter.AtlasDownload(deckName));
            Debug.Log("Yield Finished for: " + deckName);
            currentDeck = atlasConverter.getDeck(deckName);
            deckCardSize = currentDeck.cardSize;
            //AskAuth();
            //Card count at -1 due to the last card allways being a generic card back
            for (int i = 0; i < currentDeck.cardCount - 1; i++)
            {
                if (!string.IsNullOrWhiteSpace(deckName))
                {
                    int gID = relayRoom.GetComponent<NetworkRoom>().getGlobalIndex();
                    generateTexture(i);

                    Card card = new Card();
                    card.cardID = i;
                    card.globalID = gID;
                    card.cardName = currentDeck.cards[i];
                    card.cardDeck = deckName;
                    card.cardSize = currentDeck.cardSize;
                    addCard(card, true);
                }

            }
        }
        else
        {
            yield return new WaitUntil( () => cardList.Count > 0 );
            Debug.Log("Cardlist count: " + cardList.Count);
            Debug.Log("Cardlist element 0: " + cardList[0]);
            textureFalseCard(GetComponentInChildren<FakeCard>().gameObject, cardList[0]);
        }

        //CmdSyncDeck(localPlayerRef.localPlayer.GetComponent<NetworkIdentity>());
        CmdSyncDeck(NetworkClient.localPlayer);

    }

    public void shuffle()
    {
        Fisher_Yates_CardDeck_Shuffle();
    }


    private void OnCollisionEnter(Collision collision)
    {
        GameObject temp = collision.gameObject;

        if (temp.tag == "Card" )
        {
            //Do not use colision if from another network match (Server Only)
            //if (temp.GetComponent<NetworkMatchChecker>().matchId != GetComponent<NetworkMatchChecker>().matchId) return;
            if (temp.GetComponent<NetworkMatch>().matchId != GetComponent<NetworkMatch>().matchId) return;

            //Debug.Log("Collision Card");
            CmdAddCard(temp.GetComponent<CardBehaviour>().getCardDetails());
            temp.GetComponent<BoxCollider>().enabled = false;
            temp.SetActive(false);
            //if (localInteract) Destroy(temp);
            //Destroy(temp);
        }

        if(temp.tag == "Deck")
        {
            //Do not use colision if from another network match (Server Only)
            //if (temp.GetComponent<NetworkMatchChecker>().matchId != GetComponent<NetworkMatchChecker>().matchId) return;
            if (temp.GetComponent<NetworkMatch>().matchId != GetComponent<NetworkMatch>().matchId) return;

            if (isCardLifted)
            {
               // Debug.Log("Collision Deck");
                mergeDecks(temp.GetComponent<DeckScript>().deckSize, temp.GetComponent<DeckScript>().cardList);
                Destroy(temp);
            }            
        }

        if(temp.tag == "Table")
        {
            deckDrag = false;
            isCardLifted = false;
            spawned = false;
        }
        //CmdRemoveAuth();
    }

    private void OnMouseUp()
    {
        Debug.Log("Mouse Released");
        /*
        if (!GetComponent<Rigidbody>().useGravity)
            GetComponent<Rigidbody>().useGravity = true;
        */
        //CmdGravControl(true);
        cardDrop();
        if (drawnCard != null) drawnCard.GetComponent<CardBehaviour>().cardDrop();
        //drawnCard = null;
        isHandled = true;// reset the timer for the next button press
        cardSpawned = false;
        Cursor.visible = true;
        
        localHeld = false;
        if (deckSize <= 0) CmdDeleteDeck();
    }

    private void OnMouseDrag()
    {
        
        if ((Time.time > downTime + waitTime) && !isHandled )
        {
            localHeld = true;
            deckDrag = true;
            isCardLifted = true;
            cardSpawned = false;
            cardDrag();

            //Debug.Log("MouseKey was pressed and held for over " + waitTime + " secounds.");
        }
    }

    private void OnMouseExit()
    {
        if(cardSpawned && !isHandled)
        {
            Debug.Log("Card Spawn request");
            drawCard();
            lastClick = 0;
            isHandled = true;
        }
            
    }

    private void OnMouseDown()
    {
        //AskAuth();
        Debug.Log("Auth Asked Deck");
        //start recording the time when a key is pressed and held.
        Cursor.visible = false;
        downTime = Time.time;
        isHandled = false;
        cardSpawned = true;
        cardDragStart();
        //look for a double click
        if (Time.time - lastClick < 0.3)
        {
            // do something
            Debug.Log("You double clicked the target.");
            drawCard();
        }
        lastClick = Time.time;
        
    }

    [Server]
    private void mergeDecks(int size, List<Card> deckCardList)
    {
        Debug.Log("Merging decks");
        deckSize += size;
        Debug.Log(deckCardList.Count);
        foreach (var item in deckCardList)
        {
            cardList.Add(item);
            spawnFalseCard(item);
        }
        RpcMergeDecks(size, deckCardList);
    }

    private void textureFalseCard(GameObject fakeCard, Card details)
    {
        //Change to show bottom most card
        //temp.GetComponent<FakeCard>().loadTextureFront(cardList[0].textureFront);
        //temp.GetComponent<FakeCard>().loadTextureBack(details.textureBack);
        if (relayRoom == null) relayRoom = GameObject.FindGameObjectWithTag("RelayRoom");
        //temp.GetComponent<FakeCard>().loadTextureFront(relayRoom.GetComponent<NetworkRoom>().cardTexList.Find(x => x.globalID == details.globalID).textureFront);
        //temp.GetComponent<FakeCard>().loadTextureBack(relayRoom.GetComponent<NetworkRoom>().cardTexList.Find(x => x.globalID == details.globalID).textureBack);
        //temp.GetComponent<FakeCard>().loadTextureFront(relayRoom.GetComponent<NetworkRoom>().cardTexList.Find(x => x.cardID == details.cardID).textureFront);
        fakeCard.GetComponent<FakeCard>().loadTextureFront(relayRoom.GetComponent<NetworkRoom>().getCardTexture(details.cardDeck, details.cardID, false));
        //temp.GetComponent<FakeCard>().loadTextureBack(relayRoom.GetComponent<NetworkRoom>().cardTexList.Find(x => x.cardID == details.cardID).textureBack);
        fakeCard.GetComponent<FakeCard>().loadTextureBack(relayRoom.GetComponent<NetworkRoom>().getCardTexture(details.cardDeck, details.cardID, true));

    }

    [Client]
    void sizeCard(GameObject card)
    {
        card.transform.localScale = atlasConverter.ResizeCard(deckCardSize);
    }

    private void spawnFalseCard(Card details)
    {
        FakeCard deckFake = GetComponentInChildren<FakeCard>();
        if (deckFake == null)
        {
            Debug.Log("Spawn False Card");
            GameObject temp = Instantiate(FakeCardPrefab);
            Vector3 tempPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            sizeCard(temp);

            temp.transform.position = tempPos;
            temp.transform.parent = gameObject.transform;
            textureFalseCard(temp, details);
        }
        else
        {
            Debug.Log("Adjust false card instead of spawn");
            adjustFalseCard();
        }
    }

    private void addCard(Card details, bool addToDeck)
    {
        Debug.Log("Adding card");
        if(spawned) deckSize++;
        cardList.Add(details);

        if (addToDeck)
        {
            /*
             GameObject temp = Instantiate(CardPrefab);
            temp.GetComponent<CardBehaviour>().SetCardDetails(details);
            Debug.Log(details.cardID);
            temp.GetComponent<CardBehaviour>().loadTextureFront(details.textureFront);
            temp.GetComponent<CardBehaviour>().loadTextureBack(details.textureBack);

            cardPool.cards.Add(temp);
            temp.SetActive(false);
            */
            Debug.Log("Card Count in pool (AddCard): " + relayRoom.GetComponent<NetworkRoom>().cardCount);
            if (spawned && localInteract) CmdSpawnCard(details);
            //Debug.Log("Card Count in pool (AddCard): " + cardPool.cardCount); 
        }
        spawnFalseCard(details);
    }

    private void adjustFalseCard()
    {
        FakeCard deckFake = GetComponentInChildren<FakeCard>();
        Vector3 newScale = new Vector3(deckFake.gameObject.transform.localScale.x, deckSize * 40, deckFake.gameObject.transform.localScale.z);
        deckFake.gameObject.transform.localScale = newScale;

        GetComponent<TooltipTrigger>().tooltip = "" + deckSize;
        if (deckSize <= 0)
        {
            Debug.Log("Hide Deck");
            Debug.Log("Deck size: " + deckSize);
            deckFake.gameObject.SetActive(false);
            //Destroy(gameObject);
        }
    }

    private void drawCard()
    {
        Debug.Log("Drawing card");

        Vector3 tempPos = new Vector3(transform.position.x, 1.25f, transform.position.z);

        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(tempPos).z;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(mousePoint);

        /*
         GameObject temp = Instantiate(CardPrefab);
        temp.transform.position = mousePos;
        temp.GetComponent<CardBehaviour>().SetCardDetails(cardList[cardList.Count -1]);
        temp.GetComponent<CardBehaviour>().cardDragStart();
        temp.GetComponent<CardBehaviour>().isCardHeld = true;
        drawnCard = temp;
        //removeFakeCard(cardList.Count - 1);
        cardList.RemoveAt(cardList.Count - 1);
        deckSize--;
        adjustFalseCard();
         */


        Card topCard = cardList.Last();
        Debug.Log("Top card  GID: " + topCard.globalID);
        //Debug.Log("Card Sync Count Draw Card: " + cardPool.cardSync.Count);
        
        //GameObject topCardObject = cardPool.cards.Find(x => x.GetComponent<CardBehaviour>().getCardDetails().cardID == topCard.cardID);
        //GameObject topCardObject = cardPool.cardSync.Find(x => x.GetComponent<CardBehaviour>().getCardDetails().globalID == topCard.globalID);
        //CmdSetActiveCard(topCard.globalID, mousePos, localPlayerRef.localPlayer.GetComponent<NetworkIdentity>());
        CmdSetActiveCard(topCard.globalID, mousePos, NetworkClient.localPlayer.GetComponent<NetworkIdentity>());
        //topCardObject.SetActive(true);

        //EditDeck();
        /*
        topCardObject.transform.position = mousePos;

        topCardObject.GetComponent<CardBehaviour>().cardDragStart();
        topCardObject.GetComponent<CardBehaviour>().isCardHeld = true;
        topCardObject.GetComponent<CardBehaviour>().createDeck = false;
        drawnCard = topCardObject;
        */
  
    }

    private void EditDeck()
    {
        Debug.Log("Removing: " + cardList.Last().cardName);
        cardList.Remove(cardList.Last());

        deckSize--;
        adjustFalseCard();
    }

    private void Fisher_Yates_CardDeck_Shuffle()
    {

        System.Random _random = new System.Random();

        Card myGO;

        int n = cardList.Count;
        for (int i = 0; i < n; i++)
        {
            // NextDouble returns a random number between 0 and 1.
            // ... It is equivalent to Math.random() in Java.
            int r = i + (int)(_random.NextDouble() * (n - i));
            myGO = cardList[r];
            cardList[r] = cardList[i];
            cardList[i] = myGO;
        }

    }

    [Server]
    void serverSetRelay()
    {
        GameObject[] relayList = GameObject.FindGameObjectsWithTag("RelayRoom");
        for(int i = 0; i < relayList.Length; i++)
        {
            //if(relayList[i].GetComponent<NetworkMatchChecker>().matchId == GetComponent<NetworkMatchChecker>().matchId)
            if(relayList[i].GetComponent<NetworkMatch>().matchId == GetComponent<NetworkMatch>().matchId)
            {
                relayRoom = relayList[i];
                break;
            }
        }
    }

    [Client]
    void clientSetRelay()
    {
        relayRoom = GameObject.FindGameObjectWithTag("RelayRoom");
    }

    // Start is called before the first frame update
    void Start()
    {
        if(atlasConverter == null) {
            atlasConverter = GameObject.FindGameObjectWithTag("AtlasCon").GetComponent<AtlasConverter>();
        }

        Debug.Log("Deck Start");
        //cardPool = GameObject.FindGameObjectWithTag("CardPool").GetComponent<CardPool>();

        Debug.Log("Deck Atlas Converter Found");

        clientSetRelay();
        serverSetRelay();

        //localPlayerRef = GameObject.FindGameObjectWithTag("Internals").GetComponent<InternalScript>();
	    Debug.Log("Deck Local Player Ref Found");


        StartCoroutine(GenerateDeck());
        //CmdSyncDeck(localPlayerRef.localPlayer.GetComponent<NetworkIdentity>());
        //if (spawned) GenerateDeck();
    }

}