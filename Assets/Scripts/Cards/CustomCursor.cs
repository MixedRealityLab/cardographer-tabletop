using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    Vector3 offset;
    float mouseZ;

    float cardLiftHeight = 0.05f;

    public GameObject Attached;


    // Start is called before the first frame update
    void Start()
    {
        CursorDragStart();
        //Cursor.visible = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        CursorDrag();
    }

    public void CursorDragStart()
    {
        Vector3 tempPos = new Vector3(transform.position.x, cardLiftHeight, transform.position.z);
        mouseZ = Camera.main.WorldToScreenPoint(tempPos).z;
        offset = tempPos - GetMouseWorldPos();
    }

    public void CursorDrag()
    {
        Vector3 tempPos = GetMouseWorldPos() + offset;
        tempPos.y = cardLiftHeight;
        transform.position = tempPos;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mouseZ;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
