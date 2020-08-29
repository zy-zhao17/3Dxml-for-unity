using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using UnityEngine;
using System.Globalization;

/// <summary>
/// repInstance类说明：
/// 此为3drep文件的实例类。每个3drep文件中可能包含多个实例。每个实例是一个零件。零件模型由顶点数组、法线数组和三角面片下标来存储。这是图形学中非常通用的格式。
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