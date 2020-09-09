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
        Debug.Log("controller start!");
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




        //点击鼠标左键选择物体。
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 10000, Color.red, 10);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, int.MaxValue))
            {
                Debug.Log(hit.collider.gameObject.name);
                GameObject goo = hit.collider.gameObject;
                int lenn = goo.GetComponent<MeshFilter>().mesh.subMeshCount;
                for (int i = 0; i < lenn; i++)
                {
                    Material mt = goo.GetComponent<MeshRenderer>().materials[i];
                    Color color = mt.color;
                    if (color.a > 0.5)
                    {
                        color.a = 0.5f;
                        mt.color = color;
                        mt.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + 1;
                    }
                    else
                    {
                        color.a = 1;
                        mt.color = color;
                        mt.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }


                }

            }
        }

        //右键按住，相机绕轴旋转一定角度
        if (Input.GetMouseButton(1))
        {
            if (continuousR)
            {
                Vector3 mousepos_diff = Input.mousePosition - oldMousepos;
                //Camera.main.transform.localPosition += (camRight * mousepos_diff.x - camUp * mousepos_diff.y);
                Camera.main.transform.RotateAround(campos, Camera.main.transform.TransformDirection(new Vector3(0, -1, 0)), mousepos_diff.x/10f);
                Camera.main.transform.RotateAround(campos, Camera.main.transform.TransformDirection(new Vector3(1, 0, 0)), mousepos_diff.y/10f);
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













/*
 *     public GameObject cameras;
    Vector3 mouseposition;

    public float near = 20.0f;
    public float far = 100.0f;

    public float sensitivityX = 10f;
    public float sensitivityY = 10f;
    public float sensitivetyZ = 2f;
    public float sensitivetyMove = 2f;
    public float sensitivetyMouseWheel = 2f;
 * 
 * 
         //Debug.DrawRay(ray.origin, ray.direction, Color.red);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))//, int.MaxValue, 1 << LayerMask.NameToLayer("Default"))
        {
            Debug.Log("sadfad");
            //mouseposition = hit.point;
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(hit.collider.gameObject.name);
            }
        }

        //以下为鼠标滑轮功能，用来实现缩放
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            //if (Camera.main.fieldOfView <= 100)
                Camera.main.fieldOfView += 2;
            //if (Camera.main.orthographicSize <= 20)
                Camera.main.orthographicSize += 0.5F;
        }
        //Zoom in
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            //if (Camera.main.fieldOfView > 2)
                Camera.main.fieldOfView -= 2;
            //if (Camera.main.orthographicSize >= 1)
                Camera.main.orthographicSize -= 0.5F;
        }

        //鼠标右键实现视角转动，类似第一人称视角
        if (Input.GetMouseButton(1))
        {
            float rotationX = Input.GetAxis("Mouse X") * sensitivityX;
            float rotationY = Input.GetAxis("Mouse Y") * sensitivityY;
            transform.Rotate(-rotationY, rotationX, 0);
        }

        //键盘按钮←和→实现视角水平旋转
        if (Input.GetAxis("Horizontal") != 0)
        {
            float rotationZ = Input.GetAxis("Horizontal") * sensitivetyZ;
            transform.Rotate(0, 0, rotationZ);
        }
 */
