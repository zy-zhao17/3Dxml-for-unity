using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool continuousR = false;
    public bool continuousM = false;
    public Vector3 oldMousepos;


    void Start()
    {
        //Debug.Log("controller start!");
    }


    void Update()
    {
        Vector3 campos = Camera.main.transform.localPosition;
        //Vector3 camAngle = Camera.main.transform.localEulerAngles / 180 * Mathf.PI;
        Quaternion rotation = Quaternion.Euler(Camera.main.transform.localEulerAngles / 180 * Mathf.PI);
        //四元数左乘是旋转。
        Vector3 camUp = rotation * new Vector3(0, 1, 0);
        Vector3 camFront = rotation * new Vector3(1, 0, 0);
        Vector3 camRight = rotation * new Vector3(0, 0, 1);


        //右键按住，相机绕轴旋转一定角度
        if (Input.GetMouseButton(1))
        {
            if (continuousR)
            {
                Vector3 mousepos_diff = Input.mousePosition - oldMousepos;
                //Camera.main.transform.localPosition += (camRight * mousepos_diff.x - camUp * mousepos_diff.y);
                Camera.main.transform.RotateAround(campos, Camera.main.transform.TransformDirection(new Vector3(0, 1, 0)), mousepos_diff.x/10f);
                Camera.main.transform.RotateAround(campos, Camera.main.transform.TransformDirection(new Vector3(-1, 0, 0)), mousepos_diff.y/10f);
                oldMousepos += mousepos_diff;
            }
            else
            {
                oldMousepos = Input.mousePosition;
                continuousR = true;
            }
        }
        else continuousR = false;



        //中键按住，相机平动
        if (Input.GetMouseButton(2))
        {
            if (continuousM)
            {

                Vector3 mousepos_diff = Input.mousePosition - oldMousepos;
                //Camera.main.transform.Translate((camRight * mousepos_diff.x - camUp * mousepos_diff.y));
                Camera.main.transform.localPosition -= (Camera.main.transform.TransformDirection(new Vector3(1, 0, 0)) * mousepos_diff.x + Camera.main.transform.TransformDirection(new Vector3(0, 1, 0)) * mousepos_diff.y);
                oldMousepos += mousepos_diff;
            }
            else
            {
                //Debug.Log("中键按下");
                oldMousepos = Input.mousePosition;
                continuousM = true;
            }
        }
        else continuousM = false;

        //滚轮运动，相机前后移动。
        Camera.main.transform.localPosition -= Camera.main.transform.TransformDirection(new Vector3(0, 0, 1))*Input.GetAxis("Mouse ScrollWheel") * 400;
        //Camera.main.transform.TransformDirection(-camFront * Input.GetAxis("Mouse ScrollWheel") * 400);

    }

}



