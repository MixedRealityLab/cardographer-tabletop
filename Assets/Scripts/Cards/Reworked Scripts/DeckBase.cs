using System.Collections;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class DeckBase : CardBase
{
    [Header("Deck Settings")]
    [SyncVar]
    public int deckSize = 0;

    public readonly SyncList<Card> CardList = new SyncList<Card>();

    public GameObject CardPrefab;
    public GameObject FakeCard;
    public AtlasConverter AtlasObject;

    private bool CardDraw = false;
    private bool Handled = false;

    //private float LastClick = 0; Untab for doubleclick checking
    private float WaitTime = 0.5f; //wait time befor reacting
    private float DownTime; //internal time from when the key is pressed

    // Start is called before the first frame update
    protected override void Start()
    {
        if (AtlasObject == null) AtlasObject = GameObject.FindGameObjectWithTag("AtlasCon").GetComponent<AtlasConverter>();
        base.Start();
        DeckConstructorFunction();
    }

    //Deck generation
    [Server]
    void DeckConstructorFunction()
    {
        StartCoroutine(GenerateDeck());
    }
    
    IEnumerator GenerateDeck()
    {
        Debug.Log("Preset deck constructor: " + cardDetails.cardDeck);
        yield return new WaitUntil(() => AtlasObject.jsonDecks.Exists(x => x.name == cardDetails.cardDeck));
        Debug.Log("JSON deck found, continuing construction");
        List<Card> ConstructorCardList = new List<Card>();
        if (CardList.Count == 0)
        {
            
            ConstructorCardList = AtlasObject.DeckConstructor(cardDetails.cardDeck);
            foreach (var item in ConstructorCardList)
            {
                Card temp = item;
                if(temp.globalID < 0) temp.globalID = relayRoom.GetComponent<NetworkRoom>().getGlobalIndex();
                CardList.Add(temp);
            }
        }
        SetCardDetails(CardList[0]);
        deckSize = CardList.Count;
    }

    //Card Spawning
    public void DrawCardAtCursor(Vector3 parentPos)
    {
        Debug.Log("Drawing card at cursor");
        Card topCard = CardList[CardList.Count - 1];
        Debug.Log("Top card  Name to spawn: " + topCard.cardName);
        CmdActivateCard(CardList.Count - 1, parentPos, NetworkClient.localPlayer.GetComponent<NetworkIdentity>());
    }

    private void DrawCard()
    {
        Debug.Log("Drawing card");

        Vector3 tempPos = new Vector3(transform.position.x, 1.25f, transform.position.z);

        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(tempPos).z;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(mousePoint);

        Card topCard = CardList[CardList.Count -1];
        Debug.Log("Top card  Name to spawn: " + topCard.cardName);
        CmdActivateCard(CardList.Count - 1, mousePos, NetworkClient.localPlayer.GetComponent<NetworkIdentity>());

    }

    [Command]
    void CmdActivateCard(int CardToDraw, Vector3 mousePos, NetworkIdentity client)
    {
        Debug.Log("Spawning Card: " + CardList[CardToDraw].cardName);
        GameObject temp = Instantiate(CardPrefab);
        temp.GetComponent<CardBase>().SetCardDetails(CardList[CardToDraw]);
        temp.GetComponent<NetworkMatch>().matchId = GetComponent<NetworkMatch>().matchId;
        temp.transform.position = mousePos;
        NetworkServer.Spawn(temp);
        temp.GetComponent<NetworkIdentity>().AssignClientAuthority(client.connectionToClient);
        CardList.Remove(CardList[CardToDraw]);
        deckSize = CardList.Count;
        if (deckSize <= 0) Destroy(gameObject);
    }


    public void AddCardToDeck(Card card)
    {
        CardList.Add(card);
        deckSize = CardList.Count;
    }

    
    public void MergeDeck(GameObject deck)
    {
        foreach (var item in deck.GetComponent<DeckBase>().CardList)
        {
            CardList.Add(item);
        }
        deckSize = CardList.Count;
    }

    //Mouse functions
    /*
    protected override void OnMouseDown()
    {
        DownTime = Time.time;

        //Untab for doubleclick checking
        
        if (Time.time - LastClick < 0.3)
        {
            Debug.Log("Double click on deck");
            DrawCard();
        }
        LastClick = Time.time;
        
        Handled = false;
        base.OnMouseDown();
    }
    */
    public override void InteractorMenu()
    {
        AskAuthority();
        UIController.GetComponent<UIController>().ToggleDeckMenu(transform.position, gameObject);
    }

    public override void InteractorDown(Vector3 parentPos)
    {
        DownTime = Time.time;

        //Untab for doubleclick checking
        /*
        if (Time.time - LastClick < 0.3)
        {
            Debug.Log("Double click on deck");
            DrawCard();
        }
        LastClick = Time.time;
        */
        Handled = false;
        base.InteractorDown(parentPos);
    }

    /*
    protected override void OnMouseOver()
    {
        base.OnMouseOver();
        GetComponent<TooltipTrigger>().tooltip = "" + deckSize;
    }
    */
    public override void InteractorOver()
    {
        base.InteractorOver();
        //Replace with custom overlay for deck size
    }
    /*
    protected override void OnMouseDrag()
    {
        if (Time.time > DownTime + WaitTime)
        {
            Handled = true;
            if(!CardDraw)
                base.OnMouseDrag();
            CardDraw = false;
        }
        else
        {
            CardDraw = true;
        }
    }
    */
    public override void InteractorDrag(Vector3 parentPos)
    {
        if (Time.time > DownTime + WaitTime)
        {
            Handled = true;
            if (!CardDraw)
                base.InteractorDrag(parentPos);
            CardDraw = false;
        }
        else
        {
            CardDraw = true;
        }
    }
    /*
    protected override void OnMouseUp()
    {
        if (CardDraw)
        {
            CardDraw = false;
        }
        base.OnMouseUp();
    }
    */
    public override void InteractorUp()
    {
        if (CardDraw)
        {
            CardDraw = false;
        }
        base.InteractorUp();
    }
    /*
    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        if (CardDraw && !Handled)
        {
            Handled = true;
            DrawCard();
            DropAuthority();
        }
        
    }
    */
    public override void InteractorExit()
    {
        base.InteractorExit();
        if (CardDraw && !Handled)
        {
            Handled = true;
            //DrawCard();
            DropAuthority();
        }
    }

}
