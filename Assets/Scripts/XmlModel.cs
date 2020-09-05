using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using System.Globalization;
using System.IO.Compression;

/// <summary>
/// XmlModel类说明：
/// 此类为3Dxml文件的解析类。注意如果无法直接解析，需要按照zip格式解压缩后再进行解析。
/// 3Dxml类描述了各个零件模型实例之间的位置关系。
/// 此类会生成很多ModelInstance类的对象，而每个ModelInstance类的对象就对应着Unity3D中的一个零件，或部件、组件。形状相同的零部件通过不同的变换矩阵来区分不同的位置关系。
/// /// 作者：zy-zhao17@mails.tsinghua.edu.cn
/// </summary>



class XmlModel
{
    private XElement xd;

    const string xsi = "{http://www.w3.org/2001/XMLSchema-instance}";

    //private Dictionary<string, RepPart> repDict;
    //public List<ModelInstance> modelsList;
    //public ModelInstance rootInstance;//rootInstance id==1.
    public Dictionary<int, ModelInstance> InstanceDict;
    public bool loadFinished;
    public bool haveRendered;

    public XmlModel()
    {
        //repDict = new Dictionary<string, RepPart>();
        //modelsList = new List<ModelInstance>();
        InstanceDict = new Dictionary<int, ModelInstance>();
        loadFinished = false;
        haveRendered = false;

    }

    public XmlModel(string modelFilePath)
    {
        //repDict = new Dictionary<string, RepPart>();
        //modelsList = new List<ModelInstance>();
        InstanceDict = new Dictionary<int, ModelInstance>();
        loadFinished = false;
        haveRendered = false;
        LoadFromFile(modelFilePath);
    }

    public bool LoadFromFile(string modelFilePath)
    {
        if (!(File.Exists(modelFilePath)))
        {
            Debug.LogWarning("模型文件不存在!");
            return false;
        }

        FileStream fs = new FileStream(modelFilePath, FileMode.Open, FileAccess.Read);
        byte[] header=new byte[2];
        fs.Read(header, 0, 2);
        fs.Close();
        if ((header[0]==80) &&(header[1]==75))
        {
            //说明这是一个未解压缩的3dxml文件，需要将其解压缩。
            FileInfo fi = new FileInfo(modelFilePath);
            fi.MoveTo(modelFilePath+".zip");
            try
            {
                ZipFile.ExtractToDirectory(modelFilePath + ".zip", "Assets\\3dxml\\");
            }
            catch (IOException) { }
            fi = new FileInfo(modelFilePath + ".zip");
            fi.Delete();
        }

        XElement xd = XElement.Load(modelFilePath);
        IEnumerable<XElement> xels = xd.Elements().Last().Elements();
        foreach (XElement xel in xels)
        {
            string nametype = xel.Attribute(xsi + "type").Value;

            if (nametype.Equals("Reference3DType"))
            {
                //示例：<Reference3D xsi:type="Reference3DType" id="3" name="gf-3__21-90_pub_skel"/>

                Reference3D r3 = new Reference3D(xel.Attribute("name").Value);

                InstanceDict.Add(ConvertToInt(xel.Attribute("id").Value), r3);


            }
            else if (nametype.Equals("ReferenceRepType"))
            {
                //示例：<ReferenceRep xsi:type="ReferenceRepType" id="4" name="gf-3__21-90_pub_skel" format="TESSELLATED" version="1.2" associatedFile="urn:3DXML:gf-3__21-90_pub_skel.prt.3DRep">


                string filepath = "Assets\\3dxml\\" + xel.Attribute("associatedFile").Value.Split(':')[2];

                ReferenceRep rr = new ReferenceRep(xel.Attribute("name").Value, filepath);

                InstanceDict.Add(ConvertToInt(xel.Attribute("id").Value), rr);


            }
            else if (nametype.Equals("InstanceRepType"))
            {
                //<InstanceRep xsi:type="InstanceRepType" id="6" name="Part_InstanceRep">
                //<IsAggregatedBy>3</IsAggregatedBy>
                //<IsInstanceOf>4</IsInstanceOf>
                //</InstanceRep>
                int isAggregatedBy = ConvertToInt(xel.Elements().First().Value);
                int IsInstanceOf = ConvertToInt(xel.Elements().Last().Value);

                InstanceRep ir = new InstanceRep(xel.Attribute("name").Value);
                ir.child = (ReferenceRep)InstanceDict[IsInstanceOf];
                ir.parent = InstanceDict[isAggregatedBy];

                ir.child.parent = ir;
                ((Reference3D)ir.parent).childrenList.Add(ir);
                InstanceDict.Add(ConvertToInt(xel.Attribute("id").Value), ir);

            }
            else if (nametype.Equals("Instance3DType"))
            {
                //<Instance3D xsi:type="Instance3DType" id="17" name="GF-3__21-90_CABLE_PUB">
                //<IsAggregatedBy>1</IsAggregatedBy>
                //<IsInstanceOf>2</IsInstanceOf>
                //<RelativeMatrix>1 0 0 0 1 0 0 0 1 0 0 0</RelativeMatrix>
                //</Instance3D>


                int isAggregatedBy = ConvertToInt(xel.Elements().First().Value);
                int IsInstanceOf = ConvertToInt(xel.Elements().ToArray()[1].Value);
                string RelativeMatrix = xel.Elements().Last().Value;

                Instance3D i3 = new Instance3D(xel.Attribute("name").Value, RelativeMatrix);

                i3.childrenList.Add(InstanceDict[IsInstanceOf]);

                i3.parent = InstanceDict[isAggregatedBy];
                
                i3.parent.childrenList

                InstanceDict.Add(ConvertToInt(xel.Attribute("id").Value), mi);
            }



        }
        loadFinished = true;
        return true;
    }


    public bool Render(int index=-1)
    {
        return InstanceDict[1].Render();
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



