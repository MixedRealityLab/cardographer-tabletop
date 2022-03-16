using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnnotationEdit : MonoBehaviour
{
    public InputField EditField;
    public AnnotationBase AnnotationText;

    public UIController Controller;

    public string Text;

    public void SetColour(string col)
    {
        Color temp = new Color();
        switch (col)
        {
            case "Grey":
                temp = Color.grey;
                break;
            case "Green":
                temp = Color.green;
                break;
            case "Blue":
                temp = Color.blue;
                break;
            case "Red":
                temp = Color.red;
                break;
            case "Yellow":
                temp = Color.yellow;
                break;
        }
        temp.a = 0.6f;
        AnnotationText.SetColour(temp);
        Controller.ToggleAnnotationEdit(Vector3.zero);
    }

    public void Input(string s)
    {
        Text = s;
    }

    public void FinishInput()
    {
        AnnotationText.SetAnnotation(Text);
        Controller.ToggleAnnotationEdit(Vector3.zero);
    }

    public void SetTarget(GameObject anno)
    {
        AnnotationText = anno.GetComponent<AnnotationBase>();
    }

    public void Delete()
    {
        AnnotationText.AskDelete();
        Controller.ToggleAnnotationEdit(Vector3.zero);
    }

    void Start()
    {
        Controller = GameObject.FindGameObjectWithTag("UIController").GetComponent<UIController>();
    }

}
