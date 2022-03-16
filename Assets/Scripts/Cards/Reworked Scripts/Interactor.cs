using UnityEngine;

public class Interactor : MonoBehaviour
{
    public void LeftDown(Vector3 parentPos)
    {
        switch (tag)
        {
            case "Card":
                GetComponent<CardBase>().InteractorDown(parentPos);
                break;
            case "Deck":
                GetComponent<DeckBase>().InteractorDown(parentPos);
                break;
            case "Board":
                GetComponent<BoardBase>().InteractorDown(parentPos);
                break;
            case "Annotation":
                GetComponent<AnnotationBase>().InteractorDown(parentPos);
                break;
            default:
                break;
        }
    }
    public void LeftHold(Vector3 parentPos)
    {
        switch (tag)
        {
            case "Card":
                GetComponent<CardBase>().InteractorDrag(parentPos);
                break;
            case "Deck":
                GetComponent<DeckBase>().InteractorDrag(parentPos);
                break;
            case "Board":
                GetComponent<BoardBase>().InteractorDrag(parentPos);
                break;
            case "Annotation":
                GetComponent<AnnotationBase>().InteractorDrag(parentPos);
                break;
            default:
                break;
        }

    }

    public void LeftUp()
    {
        switch (tag)
        {
            case "Card":
                GetComponent<CardBase>().InteractorUp();
                break;
            case "Deck":
                GetComponent<DeckBase>().InteractorUp();
                break;
            case "Board":
                GetComponent<BoardBase>().InteractorUp();
                break;
            case "Annotation":
                GetComponent<AnnotationBase>().InteractorUp();
                break;
            default:
                break;
        }

    }

    public void RightDown()
    {
        switch (tag)
        {
            case "Card":
                GetComponent<CardBase>().InteractorMenu();
                break;
            case "Deck":
                GetComponent<DeckBase>().InteractorMenu();
                break;
            case "Board":
                GetComponent<BoardBase>().InteractorMenu();
                break;
            case "Annotation":
                GetComponent<AnnotationBase>().InteractorMenu();
                break;
            default:
                break;
        }
    }
    public void RightHold()
    {

    }
    public void RightUp()
    {

    }

    public void Over()
    {
        switch (tag)
        {
            case "Card":
                GetComponent<CardBase>().InteractorOver();
                break;
            case "Deck":
                GetComponent<DeckBase>().InteractorOver();
                break;
            case "Board":
                GetComponent<BoardBase>().InteractorOver();
                break;
            case "Annotation":
                GetComponent<AnnotationBase>().InteractorOver();
                break;
            default:
                break;
        }

    }

    public void Exit()
    {
        switch (tag)
        {
            case "Card":
                GetComponent<CardBase>().InteractorExit();
                break;
            case "Deck":
                GetComponent<DeckBase>().InteractorExit();
                break;
            case "Board":
                GetComponent<BoardBase>().InteractorExit();
                break;
            case "Annotation":
                GetComponent<AnnotationBase>().InteractorExit();
                break;
            default:
                break;
        }

    }
}
