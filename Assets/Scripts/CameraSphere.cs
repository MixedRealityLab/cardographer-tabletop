using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSphere : MonoBehaviour
{
    public GameObject rotationSphere;

    float speed;
    Vector3 spherePos;
    Vector3 sphereScale;
    Vector3 sphereRot;
    Vector3 sphereMainRot;

    // Start is called before the first frame update
    void Start()
    {
        speed = 0.02f;
        spherePos = transform.position;
        sphereScale = transform.localScale;
        sphereRot = rotationSphere.transform.eulerAngles;
        sphereMainRot = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        //Camera Rotation
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (Input.GetAxis("Mouse X") != 0)
            {
                sphereRot.y -= Input.GetAxis("Mouse X");
                transform.eulerAngles = sphereRot;
            }
            if (Input.GetAxis("Mouse Y") != 0)
            {
                sphereRot.x -= Input.GetAxis("Mouse Y");
                rotationSphere.transform.eulerAngles = sphereRot;
            }
        }

        //X,Z movement
        if (Input.GetKey(KeyCode.W))
        {
            spherePos.z += transform.forward.z * speed;
            spherePos.x += transform.forward.x * speed;
            transform.position = boundryCheck();
        }
        else if (Input.GetKey(KeyCode.S))
        {
            spherePos.z -= transform.forward.z * speed;
            spherePos.x -= transform.forward.x * speed;
            transform.position = boundryCheck();
        }

        if (Input.GetKey(KeyCode.A))
        {
            spherePos.x -= transform.right.x * speed;
            spherePos.z -= transform.right.z * speed;
            transform.position = boundryCheck();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            spherePos.x += transform.right.x * speed;
            spherePos.z += transform.right.z * speed;
            transform.position = boundryCheck();
        }

        //Zoom
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) //Backwards
        {
            sphereScale.x += speed * 20;
            sphereScale.y += speed * 20;
            sphereScale.z += speed * 20;

            transform.localScale = zoomCheck();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f) //Forward
        {
            sphereScale.x -= speed * 20;
            sphereScale.y -= speed * 20;
            sphereScale.z -= speed * 20;

            transform.localScale = zoomCheck();
        }
    }

    Vector3 zoomCheck()
    {
        if(sphereScale.x < 6)
        {
            sphereScale.x = 6;
            sphereScale.y = 6;
            sphereScale.z = 6;
        }
        else if( sphereScale.x > 20)
        {
            sphereScale.x = 20;
            sphereScale.y = 20;
            sphereScale.z = 20;
        }
        return sphereScale;
    }

    Vector3 boundryCheck()
    {

        if (spherePos.x > 7)
        {
            spherePos.x = 7f;
        }
        else if (spherePos.x < -7)
        {
            spherePos.x = -7f;
        }

        if (spherePos.y > 0)
        {
            spherePos.y = 0;
        }
        else if (spherePos.y < 0)
        {
            spherePos.y = 0f;
        }

        if (spherePos.z > 7)
        {
            spherePos.z = 7f;
        }
        else if (spherePos.z < -7)
        {
            spherePos.z = -7f;
        }
        return spherePos;
    }
}
