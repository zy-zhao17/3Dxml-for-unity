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
    private int modelRoot;
    const string xsi = "{http://www.w3.org/2001/XMLSchema-instance}";

    public Dictionary<int, ModelTree> TreeDict;
    public Dictionary<int, RepPart> RepDict;
    public bool loadFinished;
    public bool haveRendered;

    public XmlModel()
    {
        TreeDict = new Dictionary<int, ModelTree>();
        RepDict = new Dictionary<int, RepPart>();
        loadFinished = false;
        haveRendered = false;

    }

    public XmlModel(string modelFilePath)
    {
        TreeDict = new Dictionary<int, ModelTree>();
        RepDict = new Dictionary<int, RepPart>();
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

        modelRoot = ConvertToInt(xd.Elements().Last().Attribute("root").Value);
        IEnumerable<XElement> xels = xd.Elements().Last().Elements();
        foreach (XElement xel in xels)
        {
            string nametype = xel.Attribute(xsi + "type").Value;

            if (nametype.Equals("Reference3DType"))
            {
                //示例：<Reference3D xsi:type="Reference3DType" id="3" name="gf-3__21-90_pub_skel"/>

                //Reference3D r3 = new Reference3D();

                ModelTree mt = new ModelTree(xel.Attribute("name").Value);

                TreeDict.Add(ConvertToInt(xel.Attribute("id").Value), mt);


            }
            else if (nametype.Equals("ReferenceRepType"))
            {
                //示例：<ReferenceRep xsi:type="ReferenceRepType" id="4" name="gf-3__21-90_pub_skel" format="TESSELLATED" version="1.2" associatedFile="urn:3DXML:gf-3__21-90_pub_skel.prt.3DRep">


                string filepath = "Assets\\3dxml\\" + xel.Attribute("associatedFile").Value.Split(':')[2];

                RepDict.Add(ConvertToInt(xel.Attribute("id").Value), new RepPart(filepath, xel.Attribute("name").Value));

            }
            else if (nametype.Equals("InstanceRepType"))
            {
                //<InstanceRep xsi:type="InstanceRepType" id="6" name="Part_InstanceRep">
                //<IsAggregatedBy>3</IsAggregatedBy>
                //<IsInstanceOf>4</IsInstanceOf>
                //</InstanceRep>
                int isAggregatedBy = ConvertToInt(xel.Elements().First().Value);
                int isInstanceOf = ConvertToInt(xel.Elements().Last().Value);


                TreeDict[isAggregatedBy].isLeaf = true;
                TreeDict[isAggregatedBy].LeafPrt.Add(RepDict[isInstanceOf]);


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

                TreeDict[isAggregatedBy].childList.Add(TreeDict[IsInstanceOf]);
                TreeDict[isAggregatedBy].childTransferMatrixList.Add(RelativeMatrix);

            }

        }
        loadFinished = true;
        return true;
    }


    public bool Render(GameObject fathergo)
    {
        haveRendered=TreeDict[modelRoot].RenderNew(fathergo,"1 0 0 0 1 0 0 0 1 0 0 0");
        return haveRendered;
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



