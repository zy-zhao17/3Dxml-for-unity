using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using System.Globalization;

/// <summary>
/// RepPart说明：
/// 此为.3drep文件的解析类。一个3drep文件中可能包含一个或几个零件实例，保存格式为顶点加面片加rgba材质的xml格式。
/// 此类中的函数LoadFromFile的作用为加载一个3drep格式的文件并对其进行解析，解析结果保存在RepInstance实例类当中。
/// 此类中的函数Render的作用为将解析好的零件绘图到unity场景中去。
/// /// 作者：zy-zhao17@mails.tsinghua.edu.cn
/// </summary>


class RepPart
{
    public string name;
    public bool loadFinished;
    public bool haveRendered;
    public List<RepInstance> replist;
  


    //类的初始化，构造函数
    public RepPart()
    {

        loadFinished = false;
        haveRendered = false;
        replist = new List<RepInstance>();
    }

    public RepPart(string modelFilePath)
    {

        loadFinished = false;
        haveRendered = false;
        replist = new List<RepInstance>();
        LoadFromFile(modelFilePath);

        //Debug.Log("new Loaded RepPart!");
    }


    public bool LoadFromFile(string modelFilePath)
    {
        if (!(File.Exists(modelFilePath)))
        {
            Debug.LogWarning("模型文件不存在!");
            return false;
        }

        byte[] bStringValue = Encoding.UTF8.GetBytes(modelFilePath);
        int seed = int.Parse(DateTime.Now.ToString("ffffff"));
        foreach(byte b in bStringValue) seed += b;
        System.Random ran = new System.Random(seed);
        ran.NextDouble();
        ran.NextDouble();
        ran.NextDouble();
        float allcolor = (float)ran.NextDouble() + 2;
        float rProprety = (float)ran.NextDouble();
        float gProprety = (float)ran.NextDouble();
        float bProprety = (float)ran.NextDouble();
        float allSum = rProprety + gProprety + bProprety;
        rProprety = rProprety / allSum;
        gProprety = gProprety / allSum;
        bProprety = bProprety / allSum;
        Vector4 defaultColor = new Vector4(allcolor * rProprety, allcolor * gProprety, allcolor * bProprety, 1);

        name = modelFilePath.Split(new char[] { '/', '\\' }).Last().Split('.').First();

        XElement xd = XElement.Load(modelFilePath);
        IEnumerable<XElement> xdess = xd.Elements().First().Elements();


        foreach (XElement xdes in xdess)
        {
            RepInstance inst = new RepInstance();
            inst.defaultColor = defaultColor;


            XElement faces = xdes.Elements().First();
            XElement vertexBuffer = xdes.Elements().Last();

            Vector4 oldColor = new Vector4(-1, -1, -1, -1);

            foreach (XElement face in faces.Elements())
            {
                string[] strarray = face.Attribute("triangles").Value.Split(' ');
                int[] triIndexArray = Array.ConvertAll(strarray, s => int.Parse(s));
                //连着好几块颜色一样的，要直接合并，不要实例化出很多材质球，否则程序会崩掉！
                XElement rgba = face.Elements().First().Elements().First();
                Vector4 newColor;
                try
                {
                    newColor = new Vector4(ConvertToFloat(rgba.Attribute("red").Value), ConvertToFloat(rgba.Attribute("green").Value), ConvertToFloat(rgba.Attribute("blue").Value), ConvertToFloat(rgba.Attribute("alpha").Value));
                }
                catch (NullReferenceException)
                {
                    //3dxml可能存在不描述颜色的情况，这样就要给它上个默认颜色。
                    newColor = inst.defaultColor;
                }

                if (!newColor.Equals(oldColor))
                {
                    inst.colorList.Add(newColor);
                    inst.triangleRange.Add(inst.triangleList.Count);
                    oldColor = newColor;
                }

                inst.triangleList.AddRange(triIndexArray);//addrange其实就相当于append整个list上去。

            }
            inst.triangleRange.Add(inst.triangleList.Count);
            XElement vertexs = vertexBuffer.Elements().First();
            XElement normals = vertexBuffer.Elements().Last();
            string[] strvertex = vertexs.Value.Split(new char[] { ',', ' ' });
            string[] strnormal = normals.Value.Split(new char[] { ',', ' ' });

            float[] vertexArray = Array.ConvertAll(strvertex, s => float.Parse(s));
            float[] normalArray = Array.ConvertAll(strnormal, s => float.Parse(s));


            int len = vertexArray.Count();
            for (int i = 0; i < len; i += 3)
            {
                inst.vertexArrayList.Add(new Vector3(vertexArray[i], vertexArray[i + 1], vertexArray[i + 2]));
            }

            len = normalArray.Count();
            for (int i = 0; i < len; i += 3)
            {
                inst.normalArrayList.Add(new Vector3(normalArray[i], normalArray[i + 1], normalArray[i + 2]));
            }

            replist.Add(inst);
        }


        loadFinished = true;
        return true;
    }




    public bool renderNewInstance(string transformString="")
    {
        if (!loadFinished) return false;
        haveRendered = true;

        foreach(RepInstance inst in replist)
        {

            GameObject go = new GameObject(name);
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            int len = inst.colorList.Count;
            meshFilter.mesh.vertices = inst.vertexArrayList.ToArray();
            meshFilter.mesh.normals = inst.normalArrayList.ToArray();
            meshFilter.mesh.subMeshCount = len;
            meshRenderer.materials = new Material[len];

            for (int i = 0; i < len; i++)
            {
                int startpos = inst.triangleRange[i];
                int sublen = inst.triangleRange[i + 1] - startpos;

                int[] faceIndexList = inst.triangleList.GetRange(startpos, sublen).ToArray();


                meshFilter.mesh.SetTriangles(faceIndexList, i);
                meshRenderer.materials[i].color = new Color(inst.colorList[i].x, inst.colorList[i].y, inst.colorList[i].z, inst.colorList[i].w);
            }

            Transform transf = go.GetComponent<Transform>();

            if (transformString.Length > 0)
            {
                float[] matrixArray = Array.ConvertAll(transformString.Split(' '), s => float.Parse(s));
                new Vector3(matrixArray[9], matrixArray[10], matrixArray[11]);
                Quaternion newQ = Quaternion.LookRotation(new Vector3(-matrixArray[6], matrixArray[7], matrixArray[8]), new Vector3(-matrixArray[3], matrixArray[4], matrixArray[5]));
                transf.SetPositionAndRotation(new Vector3(-matrixArray[9], matrixArray[10], matrixArray[11]), newQ);
            }

            transf.localScale = new Vector3(-1, 1, 1);
        }

        //Debug.Log("渲染完成！");
        return true;



    }


    private float ConvertToFloat(string s)
    {
        return Convert.ToSingle(s, CultureInfo.InvariantCulture);
    }

}