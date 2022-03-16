using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    Camera main;
    Vector3 cameraPos;
    Vector3 cameraRot;
    float speed;
    // Start is called before the first frame update

    void Start()
    {
        main = GetComponent<Camera>();
        cameraPos = transform.position;
        cameraRot = transform.eulerAngles;
        speed = 0.25f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if(Input.GetAxis("Mouse X") != 0)
            {
                cameraRot.y += Input.GetAxis("Mouse X");
                transform.eulerAngles = cameraRot;
            }
            else if(Input.GetAxis("Mouse Y") != 0)
            {
                cameraRot.x += Input.GetAxis("Mouse Y");
                transform.eulerAngles = cameraRot;
            }
        }

        if (Input.GetKey(KeyCode.W))
        {
            cameraPos.z += transform.forward.z * speed;
            cameraPos.x += transform.forward.x * speed;
            transform.position = boundryCheck();
        }
        else if(Input.GetKey(KeyCode.S))
        {
            cameraPos.z -= transform.forward.z * speed;
            cameraPos.x -= transform.forward.x * speed;
            transform.position = boundryCheck();
        }

        if (Input.GetKey(KeyCode.A))
        {
            cameraPos.x -= transform.right.x * speed;
            cameraPos.z -= transform.right.z * speed;
            transform.position = boundryCheck();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            cameraPos.x += transform.right.x * speed;
            cameraPos.z += transform.right.z * speed;
            transform.position = boundryCheck();
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            cameraPos.y += speed;
            transform.position = boundryCheck();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
        {
            cameraPos.y -= speed;
            transform.position = boundryCheck();
        }
    }

    Vector3 boundryCheck()
    {

        if(cameraPos.x > 15)
        {
            cameraPos.x = 15f;
        }
        else if (cameraPos.x < -15)
        {
            cameraPos.x = -15f;
        }

        if(cameraPos.y > 12)
        {
            cameraPos.y = 12;
        }
        else if(cameraPos.y < 3)
        {
            cameraPos.y = 3f;
        }

        if (cameraPos.z > 15)
        {
            cameraPos.z = 15f;
        }
        else if (cameraPos.z < -15)
        {
            cameraPos.z = -15f;
        }
        return cameraPos;
    }
}
