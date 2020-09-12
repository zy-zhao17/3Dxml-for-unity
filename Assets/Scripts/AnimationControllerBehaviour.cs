using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.Xml.Linq;
using System;
using System.Globalization;

public class AnimationControllerBehaviour : MonoBehaviour
{
    //GameObject root;
    List<GameObject> currentGOList;
    List<bool> currentExpandedList;
    int currentframe=0;
    int currentmaxframe = 0;

    Dictionary<string,ASM> asmDict;

    GameObject currentGO;
    ASM currentASM;
    List<GameObject> currentChild;
    int childlen = 0;
    int childidx = 0;

    bool done = false;

    void Start()
    {
        currentGOList = new List<GameObject>();
        currentExpandedList = new List<bool>();
        currentGOList.Add(GameObject.Find("ModelGenerator").transform.GetChild(0).gameObject);
        currentExpandedList.Add(false);

        asmDict = new Dictionary<string, ASM>();
 
        ReadFromFile("XmlAnimation\\Animation.xml");

        currentChild = new List<GameObject>();

        setAlpha(GameObject.Find("ModelGenerator").transform.GetChild(0).gameObject, 0);
    }

    void Update()
    {

        if (done) return;

        currentframe++;


        if (currentframe> currentmaxframe)
        {
            currentframe = 0;
            childidx++;
            if(childidx>= currentChild.Count)//下一个装配体的动画了。或者没有动画了。
            {
                if (currentGOList.Count > 0)//如果上一个动画已经播放完成了，那么考虑下一个动画应该是播放谁。
                {
                    //第一步，如果尾部元素未展开过，则展开它。一直展到不能展下去为止。
                    while (currentExpandedList[currentExpandedList.Count - 1] == false)
                    {
                        currentExpandedList[currentExpandedList.Count - 1] = true;
                        Transform thistrans = currentGOList[currentGOList.Count - 1].transform;
                        int len = thistrans.childCount;
                        for (int i = len - 1; i >= 0; i--)
                        {
                            currentGOList.Add(thistrans.GetChild(i).gameObject);
                            currentExpandedList.Add(false);
                        }
                    }
                    //然后播放尾部并删除尾部。
                    //注意可播放的尾部一定是装配体，如果它是一个零件，就没必要播放它了，直接删除它就好。
                    //如何判断它是零件还是装配体呢：装配模板xml里面，名字写着asm的才是装配体，否则就是零件。
                    // while (currentGOList[currentGOList.Count - 1].transform.childCount <= 1)
                    while (!asmDict.ContainsKey(currentGOList[currentGOList.Count - 1].name))
                    {
                        currentExpandedList.RemoveAt(currentExpandedList.Count - 1);
                        currentGOList.RemoveAt(currentGOList.Count - 1);
                    }

                    //如果尾部是装配体，则播放它并删除它。
                    //Debug.Log(currentGOList[currentGOList.Count - 1].name);
                    currentGO = currentGOList[currentGOList.Count - 1];
                    currentExpandedList.RemoveAt(currentExpandedList.Count - 1);
                    currentGOList.RemoveAt(currentGOList.Count - 1);


                    //接下来把它的孩子们放在一个列表中，准备对它们逐一做动画。
                    childlen = currentGO.transform.childCount;
                    currentChild = new List<GameObject>();
                    for (int i = 0; i < childlen; i++)
                    {
                        currentChild.Add(currentGO.transform.GetChild(i).gameObject);
                    }
                    childidx = 0;
                    currentframe = 0;
                    //currentframe = currentmaxcframe;

                    //现在已经选中了一个新装配体。选中的装配体是currentGO。现在需要做它的装配动画
                    //首先需要找出这个新装配体对应的ASM。然后在ASM指导下才能做另一些事情。
                    currentASM = asmDict[currentGO.name];
                    currentmaxframe = currentASM.childList.First().durationFrames;
                    
                }
                else done = true;
                return;////////////////////////////////////////////////////////////////////////下个装配体的动画，不管还有没有，这里必须return掉。
            }
            else//还是这个装配体的下一个动画
            {
                currentmaxframe = currentASM.childList[childidx].durationFrames;
            }

        }

        GameObject childGO = currentChild[childidx];
        setAlpha(childGO, currentASM.childList[childidx].alphaFromTo.x * (1-(float)currentframe / currentmaxframe) + currentASM.childList[childidx].alphaFromTo.y * (float)currentframe / currentmaxframe);

        if (currentframe == 1)
        {
            childGO.transform.Translate(currentASM.childList[childidx].translateFrom);
            if (currentASM.childList[childidx].extraInstruction[0] != '#')
            {
                Debug.Log(currentASM.childList[childidx].extraInstruction);
            }
        }
        else
        {
            childGO.transform.Translate(-currentASM.childList[childidx].translateFrom / currentmaxframe);
        }




    }



    void ReadFromFile(string filePath)
    {
        XElement xd = XElement.Load(filePath);
        foreach (XElement xel in xd.Elements())
        {
            ASM asm = new ASM();
            asm.name = xel.Attribute("name").Value;
            foreach (XElement childxel in xel.Elements())
            {
                CHILD child = new CHILD();
                child.name = childxel.Attribute("name").Value;
                foreach (XElement chprop in childxel.Elements())
                {
                    switch (chprop.Name.ToString())
                    {
                        case "translateFrom":
                            string[] transtring = chprop.Value.Split(' ');
                            child.translateFrom = new Vector3(ConvertToFloat(transtring[0]), ConvertToFloat(transtring[1]), ConvertToFloat(transtring[2]));
                            //Console.WriteLine("\t\tT " + chprop.Value);
                            break;
                        case "alphaFromTo":
                            string[] astring = chprop.Value.Split(' ');
                            child.alphaFromTo = new Vector2(ConvertToFloat(astring[0]), ConvertToFloat(astring[1]));
                            //Console.WriteLine("\t\tA " + chprop.Value);
                            break;
                        case "durationFrames":
                            child.durationFrames = ConvertToInt(chprop.Value);
                            //Console.WriteLine("\t\tD " + chprop.Value);
                            break;
                        case "extraInstruction":
                            child.extraInstruction = chprop.Value;
                            //Console.WriteLine("\t\tE " + chprop.Value);
                            break;
                        default:
                            Debug.LogError("error!");
                            break;
                    }
                }
                asm.childList.Add(child);
            }
            asmDict.Add(asm.name,asm);
        }
    }


    void setAlpha(GameObject goo,float alpha)
    {

        MeshRenderer[] mats = goo.GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer mr in mats)
        {
            foreach(Material mt in mr.materials)
            {
                Color color = mt.color;
                color.a = alpha;
                mt.color = color;
                mt.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + (int)((1 - alpha) * 100);
            }

        }
    }


    private float ConvertToFloat(string s)
    {
        return Convert.ToSingle(s, CultureInfo.InvariantCulture);
    }
    private int ConvertToInt(string s)
    {
        return Convert.ToInt32(s, CultureInfo.InvariantCulture);
    }

}



class ASM
{
    public string name;
    public List<CHILD> childList;
    public ASM()
    {
        childList = new List<CHILD>();
    }
}

class CHILD
{
    public string name;
    public Vector3 translateFrom;
    public Vector2 alphaFromTo;
    public int durationFrames;
    public string extraInstruction;
    public CHILD() { }

}