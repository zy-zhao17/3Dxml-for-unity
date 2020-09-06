using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;

class ModelTree
{
    string name;

    public bool isLeaf;
    public List<RepPart> LeafPrt;

    public List<ModelTree> childList;
    public List<string> childTransferMatrixList;



    public ModelTree(string n)
    {
        name = n;
        isLeaf = false;
        LeafPrt = new List<RepPart>();
        childList = new List<ModelTree>();
        childTransferMatrixList = new List<string>();
    }



    public bool RenderNew(GameObject fatherGo,string transformString)
    {

        GameObject thisgo = new GameObject(name);
        Transform transf = thisgo.GetComponent<Transform>();
        transf.parent = fatherGo.transform;

        float[] matrixArray = Array.ConvertAll(transformString.Split(' '), s => float.Parse(s));
        new Vector3(matrixArray[9], matrixArray[10], matrixArray[11]);
        Quaternion newQ = Quaternion.LookRotation(new Vector3(-matrixArray[6], matrixArray[7], matrixArray[8]), new Vector3(-matrixArray[3], matrixArray[4], matrixArray[5]));
        //transf.SetPositionAndRotation(new Vector3(-matrixArray[9], matrixArray[10], matrixArray[11]), newQ);

        transf.localPosition = new Vector3(-matrixArray[9], matrixArray[10], matrixArray[11]);
        transf.localRotation = newQ;


        if (isLeaf)
        {
            
            foreach(RepPart prt in LeafPrt)
            {
                prt.renderNewInstance(thisgo);
            }
        }
        else
        {
            int len = childList.Count;
            if (len == 0) Debug.Log("发现不完整的模型树!");
            for(int i = 0; i < len; i++)
            {
                childList[i].RenderNew(thisgo, childTransferMatrixList[i]);
            }

        }

        return true;
    }





}

