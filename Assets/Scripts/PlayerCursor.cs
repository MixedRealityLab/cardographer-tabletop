using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum MouseFunction {LDown, LHold, LUp, RDown, RHold, RUp, Over, Exit};
public class PlayerCursor : NetworkBehaviour
{
    Plane UpPlane = new Plane(Vector3.up, 0);
    GameObject CameraObject;

    public bool isHolding;

    public GameObject playerHoverObj;
    
    float StartClick; //When the left mouse button was pressed
    public bool DeckInteract; //Say if card is being drawn from deck

    GameObject CursorObj;

    UIController UIControl;

    // Start is called before the first frame update
    public void Start()
    { 
        if (isLocalPlayer)
        {
            UIControl = GameObject.FindGameObjectWithTag("UIController").GetComponent<UIController>();
            UIControl.StopPreview();
            CameraObject = GameObject.FindGameObjectWithTag("CameraObject");
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            CursorObj = GetComponentInChildren<MeshRenderer>().gameObject;
        }
        
    }
    [Client]
    void SwitchCursourVis(bool active)
    {
        Cursor.visible = active;
        CursorObj.SetActive(!active);

    }

    void MouseInteraction(MouseFunction func)
    {
        if (!isLocalPlayer) return;
        switch (func)
        {
            case MouseFunction.LDown:
                if (UIControl.GetUIState()) break;
                if (!isHolding)
                {
                    //MouseDown
                    if (playerHoverObj != null)
                    {
                        if(playerHoverObj.tag == "Deck") StartClick = Time.time;
                        playerHoverObj.GetComponent<Interactor>().LeftDown(transform.position);
                        isHolding = !isHolding;
                        CursorObj.SetActive(false);
                    }
                }
                break;
            case MouseFunction.LHold:
                if (UIControl.GetUIState()) break;
                if (playerHoverObj != null)
                {
                    if(playerHoverObj.tag == "Deck")
                    {
                        if (Time.time > StartClick + 0.5f)
                        {
                            //Debug.Log("No Draw");
                            DeckInteract = false;
                        }
                        else
                        {
                            //Debug.Log("Draw");
                            DeckInteract = true;
                        }
                    }
                    playerHoverObj.GetComponent<Interactor>().LeftHold(transform.position);
                    //playerHoverObj.transform.position = transform.position;
                }
                break;
            case MouseFunction.LUp:
                if (UIControl.GetUIState()) break;
                if (playerHoverObj != null)
                {
                    //MouseUp
                    playerHoverObj.GetComponent<Interactor>().LeftUp();
                    //playerHoverObj.transform.parent = null;
                    playerHoverObj.transform.position = transform.position;
                    playerHoverObj = null;
                    isHolding = false;
                    CursorObj.SetActive(true);
                }
                else if (DeckInteract)
                {
                    isHolding = false;
                    DeckInteract = false;
                    CursorObj.SetActive(true);
                }
                break;
            case MouseFunction.RDown:
                if (playerHoverObj != null)
                {
                    playerHoverObj.GetComponent<Interactor>().RightDown();
                    SwitchCursourVis(UIControl.GetUIState());
                }
                else
                {
                    UIControl.CloseUi();
                }
                break;
            case MouseFunction.RHold:
                break;
            case MouseFunction.RUp:
                break;
            case MouseFunction.Over:
                if (UIControl.GetUIState()) break;
                if (playerHoverObj != null) playerHoverObj.GetComponent<Interactor>().Over();
                break;
            case MouseFunction.Exit:
                if (UIControl.GetUIState()) break;
                if (playerHoverObj != null) playerHoverObj.GetComponent<Interactor>().Exit();
                break;
            default:
                break;
        }
    }

    public void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKey(KeyCode.Z)) UIControl.MoveAndAdjustPreviewCamera(gameObject.transform.position);
        else if (!Input.GetKey(KeyCode.Z)) UIControl.StopPreview();
        if (Input.GetKeyDown(KeyCode.Tab)) SwitchCursourVis(!Cursor.visible);
        if (Input.GetKeyDown(KeyCode.Mouse0)) MouseInteraction(MouseFunction.LDown);
        if (Input.GetKeyDown(KeyCode.Mouse1)) MouseInteraction(MouseFunction.RDown);
        if (isHolding) MouseInteraction(MouseFunction.LHold);
        if (Input.GetKeyUp(KeyCode.Mouse0)) MouseInteraction(MouseFunction.LUp);
    }

    public void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            transform.rotation = CameraObject.transform.rotation;
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (UpPlane.Raycast(ray, out distance))
            {
                Vector3 cursorPos = ray.GetPoint(distance);
                if (isHolding && !DeckInteract) cursorPos.y = 1.1f;
                else cursorPos.y = 0.1f;
                transform.position = cursorPos;
                //transform.position = Vector3.Lerp(transform.position, ray.GetPoint(distance), Time.fixedDeltaTime);
            }
        }
    }
    private void InteractorCollision(GameObject InteractorObj)
    {
        Debug.Log("Collision with: "+ InteractorObj.tag);
        if (!isHolding)
        {
            Debug.Log("Switching interactor obj to: " + InteractorObj.tag);
            playerHoverObj = InteractorObj;
            playerHoverObj.GetComponent<Interactor>().Over();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Card":
            case "Deck":
            case "Board":
            case "Annotation":
                InteractorCollision(other.gameObject);
                break;
            default:
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case "Card":
            case "Deck":
            case "Board":
            case "Annotation":
                MouseInteraction(MouseFunction.Over);
                break;
            default:
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        switch (other.tag)
        {
            case "Card":
                if (!isHolding)
                {
                    if(playerHoverObj != null)
                    {
                        //Debug.Log("Collision exit from: " + other.tag);
                        MouseInteraction(MouseFunction.Exit);
                        playerHoverObj = null;
                    }
                }
                break;
            case "Deck":
                if (DeckInteract)
                {
                    playerHoverObj.GetComponent<DeckBase>().DrawCardAtCursor(transform.position);
                    MouseInteraction(MouseFunction.Exit);
                    playerHoverObj = null;
                }
                else if (!isHolding)
                {
                    if (playerHoverObj != null)
                    {
                        Debug.Log("Collision exit from: " + other.tag);
                        MouseInteraction(MouseFunction.Exit);
                        playerHoverObj = null;
                    }
                }
                break;
            case "Board":
                if (!isHolding)
                {
                    if (playerHoverObj != null)
                    {
                        //Debug.Log("Collision exit from: " + other.tag);
                        MouseInteraction(MouseFunction.Exit);
                        playerHoverObj = null;
                    }
                }
                break;
            case "Annotation":
                if (!isHolding)
                {
                    if (playerHoverObj != null)
                    {
                        //Debug.Log("Collision exit from: " + other.tag);
                        MouseInteraction(MouseFunction.Exit);
                        playerHoverObj = null;
                    }
                }
                break;
            default:
                break;
        }
    }

}