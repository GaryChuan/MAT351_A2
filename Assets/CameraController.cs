using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float sensitivity;
    
    void FixedUpdate()
    {
        if(Input.GetMouseButton(1))
        {
            float rotateHorizontal = Input.GetAxis ("Mouse X");
            float rotateVertical = Input.GetAxis ("Mouse Y");
            transform.RotateAround(Vector3.zero, -Vector3.up, rotateHorizontal * sensitivity * Time.fixedDeltaTime * 60); 
            transform.RotateAround(Vector3.zero, transform.right, rotateVertical * sensitivity * Time.fixedDeltaTime * 60);
        }
    }

    public void SetSensitivity(System.Single speed)
    {
        sensitivity = speed;
    }
}
