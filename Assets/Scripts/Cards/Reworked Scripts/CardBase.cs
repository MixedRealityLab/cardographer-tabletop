using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

[Serializable]
public struct Card
{
    public int cardID;      //Card ID in the deck
    public int globalID;    //Unique ID of the card on the server
    public string cardName;
    public string cardDeck;
    public Vector3 cardSize;
}

[Serializable]
public struct CardTextures
{
    //public int globalID; 
    public string deckName;
    public int cardID;
    public Texture2D textureFront;
    public Texture2D textureBack;
}

public class CardBase : NetworkBehaviour
{
    [Header("Card Settings")]
    [SyncVar]
    public Card cardDetails;

    public Renderer cardFaceFront;
    public Renderer cardFaceBack;
    public GameObject cardGuideFront;
    public GameObject cardGuideBack;
    public GameObject DeckPrefab;
    public GameObject AnnotationPrefab;
    protected GameObject relayRoom;
    protected GameObject zoomDisplay;
    protected GameObject UIController;

    Vector3 targetAngles;
    float mouseZ;

    protected bool textured = false;
    protected bool lifted = false;
    protected bool Flippable = true;
    [SyncVar]
    public bool Locked = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if(cardGuideFront != null) cardGuideFront.SetActive(false);
        if(cardGuideBack != null) cardGuideBack.SetActive(false);
        setClientUIController();
        SetRelayClient();
        SetRelayServer();
        ClientFunctions();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftAlt)) Zoom(false);
    }

    //UI Interactions ---------------------------------------------------------------------------------------------
    [Client]
    void setClientUIController()
    {
        UIController = GameObject.FindGameObjectWithTag("UIController");
    }
    [Client]
    public void AskCreateAnnotation(string text, Vector3 pos)
    {
        CmdCreateAnnotation(text, Vector3.zero);
        DropAuthority();
    }

    [Command]
    protected void CmdCreateAnnotation(string text, Vector3 pos)
    {
        Debug.Log("Spawn annotation");
        GameObject temp = Instantiate(AnnotationPrefab);
        if(pos == Vector3.zero)
        {
            temp.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        }
        else
        {
            temp.transform.position = pos;
        }
        
        temp.GetComponent<NetworkMatch>().matchId = GetComponent<NetworkMatch>().matchId;
        temp.GetComponent<AnnotationBase>().AnnotationText = text != "" ? text : "Spawned Annotation";
        temp.GetComponent<AnnotationBase>().ParentObject = gameObject;
        NetworkServer.Spawn(temp);
    }

    [Client]
    public void AskDelete()
    {
        CmdAskDelete();
    }

    [Command]
    void CmdAskDelete()
    {
        Destroy(gameObject);
    }

    //Relay functions ---------------------------------------------------------------------------------------------
    [Server]
    void SetRelayServer()
    {
        Debug.Log("Set relay server");
        relayRoom = GameObject.FindGameObjectWithTag("RelayRoom");
        GameObject[] relayList = GameObject.FindGameObjectsWithTag("RelayRoom");
        for (int i = 0; i < relayList.Length; i++)
        {
            if (relayList[i].GetComponent<NetworkMatch>().matchId == GetComponent<NetworkMatch>().matchId)
            {
                relayRoom = relayList[i];
                break;
            }
        }
    }

    [Client]
    void SetRelayClient()
    {
        relayRoom = GameObject.FindGameObjectWithTag("RelayRoom");
    }

    [Client]
    void ClientFunctions()
    {
        StartCoroutine(LoadTextures());
    }
    //Setting card details
    [Server]
    public void SetCardDetails(Card details)
    {
        cardDetails = details;
        ResizeCard(cardDetails.cardSize);
    }
    //Mouse interactions and card movement---------------------------------------------------------------------------------------------

    public virtual void InteractorMenu()
    {
        AskAuthority();
        UIController.GetComponent<UIController>().ToggleCardMenu(transform.position, gameObject);
    }    
    /*
    protected virtual void OnMouseExit()
    {
        //Stub
    }
    */
    public virtual void InteractorExit()
    {
        SwitchAnnotationLine(false);
    }
    /*
    protected virtual void OnMouseDown()
    {
        AskAuthority();
        CardDragStart();
        Cursor.visible = false;
    }
    */
    public virtual void InteractorDown(Vector3 parentPos)
    {
        if (Locked) return;
        AskAuthority();
        CardDragStart(parentPos);
        if (transform.rotation.eulerAngles.z > 179f) CardGuide(true);
        else CardGuide(false);
        //Cursor.visible = false;
    }
    /*
    protected virtual void OnMouseUp()
    {
        CmdGravControl(true);
        Cursor.visible = true;
    }
    */
    public virtual void InteractorUp()
    {
        Debug.Log("Interactor Up");
        CmdGravControl(true);
    }
    /*
    protected virtual void OnMouseDrag()
    {
        CardDrag();
        if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(CardFlip());
        if (transform.rotation.eulerAngles.z > 179f) CardGuide(true);
        else CardGuide(false);
    }
    */
    public virtual void InteractorDrag(Vector3 parentPos)
    {
        CardDrag(parentPos);
        if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(CardFlip());
        if (transform.rotation.eulerAngles.z > 179f) CardGuide(true);
        else CardGuide(false);
    }
    /*
    protected virtual void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.F)) StartCoroutine(CardFlip());
        if (Input.GetKey(KeyCode.LeftAlt)) Zoom(true);

        if (Input.GetKey(KeyCode.I))        StartCoroutine(CardRotation(false));
        else if (Input.GetKey(KeyCode.O))   StartCoroutine(CardRotation(true));
        else if (Input.GetKeyUp(KeyCode.I) || Input.GetKeyUp(KeyCode.O)) DropAuthority();
    }
    */
    public virtual void InteractorOver()
    {
        //if (Input.GetKey(KeyCode.LeftAlt)) Zoom(true);
        //if (Input.GetKey(KeyCode.LeftAlt)) UIController.GetComponent<UIController>().MoveAndAdjustPreviewCamera(gameObject.transform.position);
        SwitchAnnotationLine(true);
        if (Locked) return;
        if (Input.GetKeyDown(KeyCode.F)) if(Flippable) StartCoroutine(CardFlip());
        if (Input.GetKey(KeyCode.I)) StartCoroutine(CardRotation(false));
        else if (Input.GetKey(KeyCode.O)) StartCoroutine(CardRotation(true));
        else if (Input.GetKeyUp(KeyCode.I) || Input.GetKeyUp(KeyCode.O)) DropAuthority();
    }

    [Client]
    protected virtual void SwitchAnnotationLine(bool toggle)
    {
        foreach (var item in GetComponentsInChildren<AnnotationBase>())
        {
            item.IsMouseOver = toggle;
        }
    }


    //True = right, False = left
    private IEnumerator CardRotation(bool dir)
    {
        if (!GetComponent<NetworkIdentity>().hasAuthority) AskAuthority();
        yield return new WaitUntil(() => GetComponent<NetworkIdentity>().hasAuthority);
        Vector3 temp = transform.eulerAngles;
        if(dir) temp.y += 10;
        else    temp.y -= 10;
        CmdUpdateRot(temp);
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mouseZ;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    public void UiCardFlip()
    {
        StartCoroutine(CardFlip());
    }

    private IEnumerator CardFlip()
    {
        AskAuthority();
        yield return new WaitUntil(() => GetComponent<NetworkIdentity>().hasAuthority);
        targetAngles = transform.eulerAngles + 180f * Vector3.forward;
        if (targetAngles.z > 360) targetAngles.z = 0f;
        CmdUpdateRot(targetAngles);
        DropAuthority();
    }

    private void CardDragStart(Vector3 parentPos)
    {
        Debug.Log("Card Drag start");
        lifted = true;
        //transform.position = parentPos;
        //CmdUpdatePos(parentPos);
        //CmdUpdatePos(transform.position);
        //if(!NetworkClient.localPlayer) CmdUpdatePos(transform.position);
        //Vector3 tempPos = new Vector3(transform.position.x, cardLiftHeight, transform.position.z);
        //mouseZ = Camera.main.WorldToScreenPoint(tempPos).z;
        //offset = tempPos - GetMouseWorldPos();
    }

    protected void CardDrag(Vector3 parentPos)
    {
        if (Locked) return;
        if (!GetComponent<NetworkIdentity>().hasAuthority) AskAuthority();
        //Vector3 tempPos = GetMouseWorldPos() + offset;
        //tempPos.y = cardLiftHeight;
        CmdGravControl(false);
        //CmdUpdatePos(tempPos);
        transform.position = parentPos;
        //CmdUpdatePos(parentPos);
        //if (!NetworkClient.localPlayer) CmdUpdatePos(parentPos);
        CmdUpdatePos(transform.position);
        //CmdUpdatePos(transform.parent.transform.position);
    }

    [Command]
    void CmdUpdatePos(Vector3 pos)
    {
        transform.position = pos;
    }

    [Command]
    void CmdUpdateRot(Vector3 rot)
    {
        transform.eulerAngles = rot;
    }

    [Command]
    protected void CmdGravControl(bool active)
    {
        if (!GetComponent<Rigidbody>()) return;
        GetComponent<Rigidbody>().useGravity = active;
        if (GetComponent<DeckBase>() == null) GetComponent<BoxCollider>().enabled = active;
        RpcGravControl(active);
    }
    
    [ClientRpc]
    void RpcGravControl(bool active)
    {
        GetComponent<Rigidbody>().useGravity = active;
        if (GetComponent<DeckBase>() == null) GetComponent<BoxCollider>().enabled = active;
    }

    void CardGuide(bool front)
    {
        if(cardGuideFront != null) cardGuideFront.SetActive(front);
        if(cardGuideBack != null) cardGuideBack.SetActive(!front);

    }
    void CardGuideClear()
    {
        if(cardGuideFront != null) cardGuideFront.SetActive(false);
        if(cardGuideBack != null) cardGuideBack.SetActive(false);
    }

    [Client]
    private void Zoom(bool active)
    {
        if (zoomDisplay == null) zoomDisplay = GameObject.FindGameObjectWithTag("Zoom");

        if (active)
        {
            zoomDisplay.GetComponent<Image>().enabled = true;
            if (transform.rotation.eulerAngles.z > 179f) zoomDisplay.GetComponent<Image>().material.mainTexture = cardFaceBack.material.mainTexture;
            else zoomDisplay.GetComponent<Image>().material.mainTexture = cardFaceFront.material.mainTexture;
        }
        else
        {
            zoomDisplay.GetComponent<Image>().enabled = false;
        }

    }

    protected void ResizeCard(Vector3 cardSize)
    {
        Vector3 size = new Vector3();
        size.y = 1f;
        size.x = (cardSize.x / 100) + 1;
        size.z = (cardSize.z / 100) + 1;
        transform.localScale = size;
    }
    //Collisions------------------------------------------------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.collider.tag)
        {
            case "Card":
                //Do not use colision if from another network match (Server Only)
                if (collision.gameObject.GetComponent<NetworkMatch>().matchId != GetComponent<NetworkMatch>().matchId) break;
                //Ignore collision if card is already on table
                if (lifted)
                {
                    switch (gameObject.tag)
                    {
                        case "Card":
                            Debug.Log("Collision with Card on Table: " + gameObject.GetComponent<CardBase>().cardDetails.cardName);
                            CmdCreateDeck(collision.gameObject, gameObject);
                            break;
                        case "Deck":
                            CmdAddToDeck(collision.gameObject);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case "Deck":
                if (collision.gameObject.GetComponent<NetworkMatch>().matchId != GetComponent<NetworkMatch>().matchId) break;
                if (lifted)
                {
                    Debug.Log("Collision with Deck on Table");
                    switch (gameObject.tag)
                    {
                        case "Card":
                            CmdAddToDeck(collision.gameObject);
                            break;
                        case "Deck":
                            CmdMergeDecks(collision.gameObject, gameObject);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case "Table":
                Debug.Log("Collision with Table");
                DropAuthority();
                CardGuideClear();
                lifted = false;
                break;
            default:
                break;
        }
        
    }

    [Command]
    void CmdAddToDeck(GameObject deck)
    {
        Debug.Log("Collision add to deck");
        deck.GetComponent<DeckBase>().AddCardToDeck(cardDetails);
        Destroy(gameObject);
    }

    [Command]
    void CmdMergeDecks(GameObject deckOnTable, GameObject droppedDeck)
    {
        deckOnTable.GetComponent<DeckBase>().MergeDeck(droppedDeck);
        Destroy(droppedDeck);
    }

    [Command]
    protected void CmdCreateDeck(GameObject cardOnTable, GameObject droppedCard)
    {
        Debug.Log("Spawn deck from cards");
        GameObject temp = Instantiate(DeckPrefab);
        temp.transform.position = cardOnTable.transform.position;
        temp.GetComponent<NetworkMatch>().matchId = GetComponent<NetworkMatch>().matchId;
        temp.GetComponent<DeckBase>().AddCardToDeck(cardOnTable.GetComponent<CardBase>().cardDetails);
        temp.GetComponent<DeckBase>().AddCardToDeck(droppedCard.GetComponent<CardBase>().cardDetails);
        temp.GetComponent<DeckBase>().SetCardDetails(cardOnTable.GetComponent<CardBase>().cardDetails);
        Destroy(cardOnTable);
        Destroy(droppedCard);
        NetworkServer.Spawn(temp);
    }

    private void OnCollisionExit(Collision collision)
    {
        switch (collision.collider.tag)
        {
            case "Table":
                break;
            default:
                break;
        }
    }

    //Texture Functions
    [Client]
    protected IEnumerator LoadTextures()
    {
        Debug.Log("Is Textured Bool: " + textured);
        yield return new WaitUntil( () => cardDetails.cardName != "");
        Debug.Log("Card Coroutine Load Tex start: " + cardDetails.cardName);
        yield return new WaitUntil( () => relayRoom = GameObject.FindGameObjectWithTag("RelayRoom"));
        Debug.Log("Card Coroutine Load Tex first yield: " + cardDetails.cardName);
        relayRoom.GetComponent<NetworkRoom>().DownloadRef(cardDetails.cardDeck);
        yield return new WaitUntil(() => relayRoom.GetComponent<NetworkRoom>().IsRefCachec(cardDetails.cardDeck));
        Debug.Log("Card Coroutine Load Tex second yield");
        relayRoom.GetComponent<NetworkRoom>().DeckFromRef(cardDetails.cardDeck);
        yield return new WaitUntil(() => relayRoom.GetComponent<NetworkRoom>().getCardTexture(cardDetails.cardDeck, cardDetails.cardID, false) != null);
        Debug.Log("Card Coroutine Load Tex third yield: " + cardDetails.cardName);
        cardFaceFront.material.mainTexture = relayRoom.GetComponent<NetworkRoom>().getCardTexture(cardDetails.cardDeck, cardDetails.cardID, false);
        cardFaceFront.material.mainTextureScale = new Vector2(-1f, -1f);
        cardFaceBack.material.mainTexture = relayRoom.GetComponent<NetworkRoom>().getCardTexture(cardDetails.cardDeck, cardDetails.cardID, true);
        textured = true;
    }

    [Client]
    private void OnEnable()
    {
        if (!textured) StartCoroutine(LoadTextures());
    }

    //Authority Functions---------------------------------------------------------------------------------------------------

    protected void AskAuthority()
    {
        CmdAskAuthority(NetworkClient.localPlayer);
    }

    [Command(requiresAuthority = false)]
    void CmdAskAuthority(NetworkIdentity client)
    {
        if (GetComponent<NetworkIdentity>().hasAuthority)
        {
            Debug.Log("Object already owned");
            return;
        }
        Debug.Log("Auth set to: " + client.name);
        GetComponent<NetworkIdentity>().AssignClientAuthority(client.connectionToClient);
    }

    protected void DropAuthority()
    {
        CmdDropAuthority();
    }

    [Command]
    void CmdDropAuthority()
    {
        Debug.Log("Dropping auth");
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }
}
