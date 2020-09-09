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
/// 此类中的函数Render的作用为将解析好的零件绘图到unity场景中去。注意：Render函数必须同步调用，不能在后台进程中调用。
/// /// 作者：zy-zhao17@mails.tsinghua.edu.cn
/// </summary>


class RepPart
{
    public string name;
    public bool loadFinished;
    //public bool haveRendered;
    public List<RepInstance> replist;

    static System.Random ran = new System.Random();



    //类的初始化，构造函数
    public RepPart()
    {

        loadFinished = false;
        //haveRendered = false;
        replist = new List<RepInstance>();
    }

    public RepPart(string modelFilePath,string nam="")
    {

        loadFinished = false;
        //haveRendered = false;
        replist = new List<RepInstance>();
        LoadFromFile(modelFilePath);
        if (nam.Count() > 0)
        {
            name = nam;
        }

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
        //int seed = int.Parse(DateTime.Now.ToString("ffffff"));
        //foreach(byte b in bStringValue) seed += b;
        
        ran.NextDouble();
        ran.NextDouble();
        ran.NextDouble();



        float H = (float)ran.NextDouble();
        float S = 0.5f * (float)ran.NextDouble();
        float V = 0.7f + 0.3f * (float)ran.NextDouble();
        Color c= Color.HSVToRGB(H,S,V);



        Vector4 defaultColor = new Vector4(c.r, c.g, c.b, 1);

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




    public bool renderNewInstance(GameObject fatherGo)
    {
        if (!loadFinished) return false;


        foreach(RepInstance inst in replist)
        {
            GameObject go = new GameObject(name);
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = go.AddComponent<MeshCollider>();


            int len = inst.colorList.Count;
            meshFilter.mesh.vertices = inst.vertexArrayList.ToArray();
            meshFilter.mesh.normals = inst.normalArrayList.ToArray();
            meshFilter.mesh.subMeshCount = len;
            meshRenderer.materials = new Material[len];

            meshCollider.sharedMesh = meshFilter.mesh;

            for (int i = 0; i < len; i++)
            {
                int startpos = inst.triangleRange[i];
                int sublen = inst.triangleRange[i + 1] - startpos;

                int[] faceIndexList = inst.triangleList.GetRange(startpos, sublen).ToArray();
                meshFilter.mesh.SetTriangles(faceIndexList, i);

                Material mt = meshRenderer.materials[i];/////////////////此处为什么不会报null空指针错，就很奇怪，但是它确实没有报null。
                mt.SetOverrideTag("RenderType", "Transparent"); //fade mode
                mt.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mt.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mt.SetInt("_ZWrite", 1);
                mt.DisableKeyword("_ALPHATEST_ON");
                mt.EnableKeyword("_ALPHABLEND_ON");
                mt.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mt.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                mt.color = new Color(inst.colorList[i].x, inst.colorList[i].y, inst.colorList[i].z, inst.colorList[i].w);


            }

            Transform transf = go.GetComponent<Transform>();
            transf.parent= fatherGo.transform;//////////////////////设置装配体的层次结构关系。
            transf.localScale = new Vector3(-1, 1, 1);
            transf.localPosition = new Vector3(0, 0, 0);
            transf.localRotation = new Quaternion(0, 0, 0, 0);

        }

        return true;



    }


    private float ConvertToFloat(string s)
    {
        return Convert.ToSingle(s, CultureInfo.InvariantCulture);
    }


}





/// <summary>
/// repInstance类说明：
/// repInstance类只能由RepPart类生成和使用。
/// 此为3drep文件的实例类，只能由RepPart类来生成。每个3drep文件中可能包含多个实例。每个实例是一个零件。零件模型由顶点数组、法线数组和三角面片下标来存储。这是图形学中非常通用的格式。
/// 此类仅供存储顶点面片用，无数据处理功能。数据处理功能在RepPart类中实现。
/// /// 作者：zy-zhao17@mails.tsinghua.edu.cn
/// </summary>


class RepInstance
{
    public List<Vector3> vertexArrayList; // 顶点数组，每个文件共用一组
    public List<Vector3> normalArrayList; // 法线数组，每个文件共用一组
    public List<int> triangleList;
    public List<Vector4> colorList;
    public List<int> triangleRange;
    public Vector4 defaultColor;

    public RepInstance()
    {
        vertexArrayList = new List<Vector3>(); // 顶点数组，每个文件共用一组
        normalArrayList = new List<Vector3>(); // 法线数组，每个文件共用一组
        triangleList = new List<int>();
        colorList = new List<Vector4>();
        triangleRange = new List<int>();//这个列表，存储顶点列表中的哪一段到哪一段用哪一种材质(目前材质仅区分rgba)。

    }


}