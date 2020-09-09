using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBehaviour : MonoBehaviour
{
    GameObject go;


    // Start is called before the first frame update
    void Start()
    {
        go = GameObject.Find("ModelGenerator");
        //printtree(go);
    }

    // Update is called once per frame
    void Update()
    {
        
        Transform transf = go.GetComponent<Transform>();
        //transf.localPosition = transf.localPosition + new Vector3(1, 1, 1);

        //settree(go);

        //Debug.Log(transf.localPosition);

    }


    void printtree(GameObject go, int layer = 0)
    {
        string str = "";
        for (int i = 0; i < layer; i++) str += "\t";
        Debug.Log(str+go.name);
        int len = go.transform.childCount;
        for(int i = 0; i < len; i++)
        {
            printtree(go.transform.GetChild(i).gameObject, layer + 1);
        }

    }

    void settree(GameObject goo, int layer = 0)
    {
        int len = goo.transform.childCount;

        for (int i = 0; i < len; i++)
        {
            settree(goo.transform.GetChild(i).gameObject, layer + 1);
        }

        if (len == 0)
        {
            try
            {
                int lenn = goo.GetComponent<MeshFilter>().mesh.subMeshCount;
                for(int i = 0; i < lenn; i++)
                {
                    Material mt = goo.GetComponent<MeshRenderer>().materials[i];
                    Color color = mt.color;
                    if (color.a > 0.5) color.a = color.a * 0.99f;
                    mt.color = color;
                }

            }
            catch (MissingComponentException) { }

        }



    }








}
