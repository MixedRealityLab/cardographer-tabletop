using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class AnnotationBase : CardBase
{
    [Header("Annotation Settings")]
    public Text Annotation;
    [SyncVar]
    public string AnnotationText;
    [SyncVar]
    public GameObject ParentObject;
    [SyncVar]
    public Color AnnotationColourTag;
    public Image AnnotationImageColour;
    public LineRenderer AnnotationLine;
    public bool IsMouseOver = false;

    protected override void Start()
    {
        Flippable = false;
        if (AnnotationText != "") Annotation.text = AnnotationText;
        else Annotation.text = "Enter new annotation";
        if (ParentObject != null)
        {
            transform.parent = ParentObject.transform;
        }
        if (AnnotationColourTag != Color.clear) AnnotationImageColour.color = AnnotationColourTag;
        else
        {
            Color spawn = Color.grey;
            spawn.a = 0.6f;
            AnnotationImageColour.color = spawn;
        }
        base.Start();
    }
    private void FixedUpdate()
    {
        if (ParentObject != null)
        {
            transform.localRotation = ParentObject.transform.rotation;
            transform.position = new Vector3(transform.position.x, 1, transform.position.z);
            AnnotationLine.enabled = IsMouseOver;
            if (IsMouseOver)
            {
                //Draw line between annotation and object if moused over, either on the annotation or parent object
                AnnotationLine.SetPosition(0, gameObject.transform.position);
                AnnotationLine.SetPosition(1, ParentObject.transform.position);
            }
        }
    }

    //Interaction Functions
    public override void InteractorDown(Vector3 parentPos)
    {
        base.InteractorDown(parentPos);
    }

    public override void InteractorMenu()
    {
        AskAuthority();
        UIController.GetComponent<UIController>().ToggleAnnotationEdit(transform.position, gameObject);
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
        DropAuthority();
    }

    public override void InteractorExit()
    {
        base.InteractorExit();
        DropAuthority();
    }

    protected override void SwitchAnnotationLine(bool toggle)
    {
        IsMouseOver = toggle;
    }

    //Annotation Functions
    [Client]
    public void SetAnnotation(string s)
    {
        CmdSetAnnotation(s);
        DropAuthority();
    }
    [Command]
    void CmdSetAnnotation(string s)
    {
        AnnotationText = s;
        RpcSetAnnotation(s);
    }

    [ClientRpc]
    void RpcSetAnnotation(string s)
    {
        Annotation.text = s;
    }

    //Annotation Colour functions
    [Client]
    public void SetColour(Color col)
    {
        CmdSetColour(col);
        DropAuthority();
    }

    [Command]
    void CmdSetColour(Color col)
    {
        AnnotationColourTag = col;
        RpcSetColour(col);
    }

    [ClientRpc]
    void RpcSetColour(Color col)
    {
        AnnotationImageColour.color = col;
    }
}
