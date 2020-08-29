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
    private Dictionary<string, RepPart> repDict;
    public List<ModelInstance> modelsList;
    public bool loadFinished;
    public bool haveRendered;

    public XmlModel()
    {
        repDict = new Dictionary<string, RepPart>();
        modelsList = new List<ModelInstance>();
        loadFinished = false;
        haveRendered = false;

    }

    public XmlModel(string modelFilePath)
    {
        repDict = new Dictionary<string, RepPart>();
        modelsList = new List<ModelInstance>();
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
            if (xel.Name.ToString().Split('}')[1].Equals("Instance3D"))
            {
                string repFilePath = "Assets\\3dxml\\" + xel.Attribute("name").Value + ".prt.3DRep";
                if (File.Exists(repFilePath))
                {
                    string matrixString = xel.Elements().Last().Value;
                    if (!repDict.ContainsKey(repFilePath))
                    {
                        repDict.Add(repFilePath, new RepPart(repFilePath));
                    }
                    modelsList.Add(new ModelInstance(repDict[repFilePath], matrixString));
                }
            }
        }
        loadFinished = true;
        return true;
    }


    public bool Render()
    {
        
        foreach(ModelInstance mi in modelsList)
        {
            mi.Render();
        }
        haveRendered = true;
        return true;


    }

}



