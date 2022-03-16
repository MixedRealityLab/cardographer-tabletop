using UnityEngine;
using Mirror;
using System.Collections;

public class BoardBase : CardBase
{
    [Header("Deck Settings")]
    [SyncVar]
    public Board BoardDetails;

    public override void InteractorMenu()
    {
        AskAuthority();
        UIController.GetComponent<UIController>().ToggleBoardMenu(transform.position, gameObject);
    }

    public override void InteractorDown(Vector3 parentPos)
    {
        base.InteractorDown(parentPos);
    }

    public override void InteractorOver()
    {
        base.InteractorOver();
    }

    public override void InteractorDrag(Vector3 parentPos)
    {
        base.InteractorDrag(parentPos);
    }
    public override void InteractorUp()
    {
        base.InteractorUp();
    }

    public override void InteractorExit()
    {
        base.InteractorExit();
    }

    public void ToggleBoardLock()
    {
        CmdToggleBoardLock();
        DropAuthority();
    }
    [Command]
    void CmdToggleBoardLock()
    {
        Locked = !Locked;
    }

    IEnumerator LoadBoardTexture()
    {
        Debug.Log("Is Textured Bool: " + textured);
        yield return new WaitUntil(() => BoardDetails.boardName != "");

        Debug.Log("Board Coroutine Load Tex start: " + BoardDetails.boardName);
        yield return new WaitUntil(() => relayRoom = GameObject.FindGameObjectWithTag("RelayRoom"));
        Debug.Log("Board Coroutine Load Tex first yield: " + BoardDetails.boardName);
        
        Davinci
                .get()
                .load(BoardDetails.boardImageURL[0])
                .setCached(true)
                .into(cardFaceFront)
                .withEndAction(() =>
                {
                    textured = true;
                    cardFaceFront.material.mainTextureScale = new Vector2(-1f, -1f);
                })
                .start();
        
    }

    [Client]
    void ClientFunctions()
    {
        StartCoroutine(LoadTextures());
    }

    [Client]
    private void OnEnable()
    {
        if (!textured) StartCoroutine(LoadBoardTexture());
    }

    [Server]
    private void ServerFunctions()
    {
        if(BoardDetails != null) ResizeCard(new Vector3(BoardDetails.boardSize[1] / 10, 1, BoardDetails.boardSize[0] / 10));
    }

    private void Start()
    {
        Flippable = false;
        ClientFunctions();
        ServerFunctions();
        base.Start();
    }


}
