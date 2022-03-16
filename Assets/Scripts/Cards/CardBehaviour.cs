using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class CardBehaviour : NetworkBehaviour
{
    public GameObject DeckPrefab;

    //public Card cardDetails;

    [SyncVar]
    public int cardID;
    [SyncVar]
    public int globalID;
    [SyncVar]
    public string cardName;
    [SyncVar]
    public string originDeckName;

    [SyncVar]
    public Vector3 cardSize;

    //Room object
    public GameObject relayRoom;

    Vector3 offset;
    float mouseZ;

    float cardLiftHeight = 1.25f;

    bool isCardSpawned = true;
    bool isCardFlip;
    public bool isCardLifted;
    public bool isCardHeld = false;
    public bool createDeck = false;

    protected bool localInteract = false;

    protected Coroutine textureLoader;

    float smooth = 1f;

    private Vector3 targetAngles;
    private GameObject zoomDisplay;

    //protected InternalScript localPlayerRef;

    protected Renderer[] objRender;

    private bool textured = false;

    //Network Commands
    [Command(requiresAuthority = false)]
    void CmdUpdatePos(float x, float y, float z)
    {
        Vector3 newPos = new Vector3(x, y, z);
        transform.position = newPos;
        RpcUpdatePos(newPos);
    }

    [Command(requiresAuthority = false)]
    void CmdUpdateRot(float x, float y, float z)
    {
        Vector3 newRot = new Vector3(x, y, z);
        transform.eulerAngles = newRot;
        RpcUpdateRot(newRot);
    }

    [ClientRpc]
    void RpcUpdateRot(Vector3 rot)
    {
        transform.eulerAngles = rot;
    }

    [ClientRpc]
    void RpcUpdatePos(Vector3 pos)
    {
        transform.position = pos;
    }

    [Command(requiresAuthority = false)]
    protected void CmdGravControl(bool active)
    {
        GetComponent<Rigidbody>().useGravity = active;
        if(GetComponent<DeckScript>() == null) GetComponent<BoxCollider>().enabled = active;
        RpcGravControl(active);
    }

    [ClientRpc]
    void RpcGravControl(bool active)
    {
        GetComponent<Rigidbody>().useGravity = active;
        if (GetComponent<DeckScript>() == null) GetComponent<BoxCollider>().enabled = active;
    }


    [Command(requiresAuthority = false)]
    protected void CmdCreateDeck(Vector3 pos, Vector3 size)
    {
        Debug.Log("Spawn deck from cards");
        GameObject temp = Instantiate(DeckPrefab);
        temp.GetComponent<DeckScript>().deckCardSize = size;
        temp.transform.position = pos;
        //temp.GetComponent<NetworkMatchChecker>().matchId = GetComponent<NetworkMatchChecker>().matchId;
        temp.GetComponent<NetworkMatch>().matchId = GetComponent<NetworkMatch>().matchId;
        NetworkServer.Spawn(temp);
    }

    [Command(requiresAuthority = false)]
    protected void CmdAskAuthority(NetworkIdentity clientID)
    {
        if (!GetComponent<NetworkIdentity>().hasAuthority)
        {
            GetComponent<NetworkIdentity>().AssignClientAuthority(clientID.connectionToClient);
            Debug.Log("Auth set to Client conn = " + clientID.connectionToClient);
        }
        else
        {
            Debug.Log("Auth already set");
        }
        

    }
    [Command]
    protected void CmdRemoveAuth()
    {
        Debug.Log("Removing Auth");
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
    }

    //Network Commands

    /// <summary>
    /// Function for the Card Context Menu
    /// </summary>
    /// <param name="opt">1: Flip</param>
    public void CardInteract(int opt)
    {
        switch (opt)
        {
            case 1:
                cardFlip();
                break;
            case 2:
                break;
            case 3:
                break;
            default:
                break;
        }
    }

    public void SetCardDetails(Card details)
    {
        Debug.Log("Set Card");
        //cardDetails = new Card();
        //cardDetails = details;
        cardID = details.cardID;
        cardName = details.cardName;
        globalID = details.globalID;
        originDeckName = details.cardDeck;
        cardSize = details.cardSize;
    }

    public Card getCardDetails()
    {
        Debug.Log("Getting card");
        Card cardDetails = new Card();
        cardDetails.cardID = cardID;
        cardDetails.cardName = cardName;
        cardDetails.globalID = globalID;
        cardDetails.cardDeck = originDeckName;
        cardDetails.cardSize = cardSize;
        return cardDetails;
    }

    public void cardDragStart()
    {
        Debug.Log("Card Drag start");
        Vector3 tempPos = new Vector3(transform.position.x, cardLiftHeight, transform.position.z);
        mouseZ = Camera.main.WorldToScreenPoint(tempPos).z;
        offset = tempPos - GetMouseWorldPos();

        GetComponent<Rigidbody>().useGravity = false;
        CmdGravControl(false);
    }

    public void cardDrag()
    {
        //AskAuth();
        Vector3 tempPos = GetMouseWorldPos() + offset;
        tempPos.y = cardLiftHeight;
        transform.position = tempPos;
        CmdUpdatePos(
                    transform.position.x,
                    transform.position.y,
                    transform.position.z
                    );
    }

    public void cardDrop()
    {
        /*
        if (!GetComponent<Rigidbody>().useGravity)
            GetComponent<Rigidbody>().useGravity = true;*/
        isCardHeld = false;
        CmdGravControl(true);
        //isCardLifted = false;
    }

    //Private Functions
    private void zoomCard(bool active)
    {//Client Only
        if(zoomDisplay == null)
        {
            zoomDisplay = GameObject.FindGameObjectWithTag("Zoom");
        }

        if (active)
        {
            zoomDisplay.GetComponent<Image>().enabled = true;
            Renderer[] temp = GetComponentsInChildren<Renderer>();
            if (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 270)
            {
                zoomDisplay.GetComponent<Image>().material.mainTexture = temp[2].material.mainTexture;
            }
            else
            {
                
                zoomDisplay.GetComponent<Image>().material.mainTexture = temp[0].material.mainTexture;
            }
        }
        else
        {
            zoomDisplay.GetComponent<Image>().enabled = false;
        }
        
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.collider.tag == "Table")
        {
            isCardLifted = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isCardHeld)
        {
            // Debug.Log(collision.collider.tag);
            GetComponent<Rigidbody>().useGravity = true;

            if(collision.collider.tag == "Reset")
            {
                Debug.Log("Hit Reset");
                Vector3 pos = transform.position;
                pos.y = 2f;
                transform.position = pos;
            }

            if (collision.collider.tag == "Table")
            {
                isCardFlip = false;
                CmdUpdateRot(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
                isCardLifted = false;
            }

            if (collision.collider.tag == "Card")
            {
                //Do not use colision if from another network match (Server Only)
                //if (collision.gameObject.GetComponent<NetworkMatchChecker>().matchId != GetComponent<NetworkMatchChecker>().matchId) return;
                if (collision.gameObject.GetComponent<NetworkMatch>().matchId != GetComponent<NetworkMatch>().matchId) return;

                //Debug.Log("Card Card Colision");
                if (localInteract && isCardLifted && !createDeck)
                {
                    createDeck = true;
                    CmdCreateDeck(collision.gameObject.transform.position, cardSize);
                    createDeck = false;
                    //Debug.Log("Card on Table");
                    //if (localInteract) Destroy(gameObject);
                }
            }
            //CmdRemoveAuth();
            localInteract = false;
        }
    }

    private void cardFlip()
    {
        
        if (transform.position.y > 0 && transform.position.y < 0.1)
        {
            Vector3 temp = transform.position;
            temp.y = cardLiftHeight;
            transform.position = temp;
        }

        targetAngles = transform.eulerAngles + 180f * Vector3.forward;
        if (targetAngles.z > 360) targetAngles.z = 0f;

        isCardFlip = true;
    }

    private void cardInteract()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            cardFlip();
        }

        if (Input.GetKey(KeyCode.U))
        {
            Vector3 temp = transform.eulerAngles;
            temp.y -= 1;
            transform.eulerAngles = temp;
            CmdUpdateRot(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        else if (Input.GetKey(KeyCode.I))
        {
            Vector3 temp = transform.eulerAngles;
            temp.y += 1;
            transform.eulerAngles = temp;
            CmdUpdateRot(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }

    private void OnMouseExit()
    {
        zoomCard(false);
        if(!isCardLifted) gameObject.GetComponentInChildren<Outline>().enabled = false;
    }

    private void OnMouseOver()
    {
        cardInteract();

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            zoomCard(true);
        }
        //If the mouseover is a deck, do not outline
        if(gameObject.GetComponent<DeckScript>() == null)
            gameObject.GetComponentInChildren<Outline>().enabled = true;
    }

    [Client]
    protected void AskAuth()
    {
        //CmdAskAuthority(localPlayerRef.localPlayer.GetComponent<NetworkIdentity>());
        CmdAskAuthority(NetworkClient.localPlayer.GetComponent<NetworkIdentity>());
    }

    private void OnMouseDown()
    {
        //AskAuth();
        cardInteract();
        cardDragStart();
        isCardLifted = true;
        localInteract = true;
        Cursor.visible = false;
        
        if (isCardLifted)
        {
            gameObject.GetComponentInChildren<Outline>().enabled = true;
            gameObject.GetComponentInChildren<Outline>().OutlineColor = Color.gray;
        }
    }

    private void OnMouseUp()
    {
        Debug.Log("Card behaviour Mouse Up");
        if (isCardLifted)
        {
            gameObject.GetComponentInChildren<Outline>().enabled = false;
            gameObject.GetComponentInChildren<Outline>().OutlineColor = Color.red;
        }

        cardDrop();
        Cursor.visible = true;
    }

    private Vector3 GetMouseWorldPos()
    {
        cardInteract();
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mouseZ;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDrag()
    {
        cardInteract();
        cardDrag();
    }


    public void loadTextureFront(Texture2D texture)
    {
        Renderer[] temp = GetComponentsInChildren<Renderer>();
        temp[0].material.mainTexture = texture;
        temp[0].material.mainTextureScale = new Vector2(-1f, -1f);
    }

    public void loadTextureBack(Texture2D texture)
    {
        Renderer[] temp = GetComponentsInChildren<Renderer>();
        temp[2].material.mainTexture = texture;
    }

    public void setLocalInteract()
    {
        localInteract = true;
    }

    public bool getLocalInteract()
    {
        return localInteract;
    }

    [Client]
    IEnumerator loadTextures()
    {
        Debug.Log("Is Textured Bool: " + textured);
        Debug.Log("Card Coroutine Load Tex start: " + cardName);
        yield return relayRoom = GameObject.FindGameObjectWithTag("RelayRoom");
        Debug.Log("Card Coroutine Load Tex first yield: " + cardName);
        yield return new WaitUntil( () => relayRoom.GetComponent<NetworkRoom>().getCardTexture(originDeckName, cardID, false) != null);
        Debug.Log("Card Coroutine Load Tex second yield: " + cardName);
        objRender[0].material.mainTexture = relayRoom.GetComponent<NetworkRoom>().getCardTexture(originDeckName, cardID, false);
        objRender[0].material.mainTextureScale = new Vector2(-1f, -1f);
        objRender[2].material.mainTexture = relayRoom.GetComponent<NetworkRoom>().getCardTexture(originDeckName, cardID, true);
        textured = true;
        transform.localScale = GameObject.FindGameObjectWithTag("AtlasCon").GetComponent<AtlasConverter>().ResizeCard(cardSize);

    }

    [Client]
    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(cardName)) if (!textured) textureLoader = StartCoroutine(loadTextures());
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start Card");
        isCardFlip = false;
        isCardLifted = false;
        //localPlayerRef = GameObject.FindGameObjectWithTag("Internals").GetComponent<InternalScript>();
        objRender = GetComponentsInChildren<Renderer>();
#if !UNITY_SERVER
        if(!string.IsNullOrEmpty(cardName)) textureLoader = StartCoroutine(loadTextures());
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (isCardFlip)
        {
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, targetAngles, smooth * (Time.deltaTime * 4));       
        }

        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            zoomCard(false);
        }

        if (isCardHeld)
        {
            cardDrag();
        }
    }
}
