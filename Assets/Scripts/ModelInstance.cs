using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// ModelInstance类说明：
/// 此类用于存储模型实例。
/// 通常情况下，一个模型（例如：同一规格的螺栓）可能在装配体上使用多次。此时利用C#语言的引用特性，无需复制出多份RepPart对象，而仅保存一份对象，并创建多个引用指向它。
/// 每创建一个ModelInstance对象，相当于创建了一个指向特定唯一RepPart零件的引用，再加一个表征自身特征的变换矩阵。
/// 这样做可以节省空间、提高运行效率，因为不必每次都去文件中读取或复制那些顶点和面片。
/// /// 作者：zy-zhao17@mails.tsinghua.edu.cn
/// </summary>
/*
class ModelInstance
{
    //bool is
    RepPart prt;
    string transformString;
    bool haveRendered;

    public ModelInstance(RepPart p,string s)
    {
        prt = p;
        transformString = s;
        haveRendered = false;
    }

    public bool Render()
    {
        if (haveRendered) return true;
        haveRendered = prt.renderNewInstance(transformString);
        return haveRendered;
    }

}
*/
class ModelInstance
{
    public string nameString;

    public ModelInstance(string s)
    {
        nameString = s;
    }

    /*
    public string transformString;
    
    public bool haveRendered;
    public bool isLeaf;
    public RepPart prt;
    public List<ModelInstance> childList;

    public ModelInstance(string namestring = "default", string prtFileString = null, string transformstring = "1 0 0 0 1 0 0 0 1 0 0 0")
    {
        transformString = transformstring;
        nameString = namestring;
        haveRendered = false;

        if (prtFileString != null)
        {
            isLeaf = true;
            prt = new RepPart(prtFileString);
            childList = null;
        }
        else
        {
            isLeaf = false;
            prt = null;
            childList = new List<ModelInstance>();
        }
    }
    
    public bool Render()
    {
        if (haveRendered) return true;


        if (isLeaf)
        {
            haveRendered = prt.renderNewInstance(transformString);
        }
        else
        {
            haveRendered = true;
            foreach (ModelInstance child in childList)
            {
                haveRendered &= child.Render();
            }
        }
        return haveRendered;

    }
    */
}


//这个Reference3D类是用来搭建模型的，一般具有复用性。比如一个模型的子模型可能是两个一样的，由于有了Reference3D的存在，建立Instance3D的时候，就可以直接从这里边来找，避免把一样的东西初始化很多次。
class Reference3D : ModelInstance
{
    //示例：<Reference3D xsi:type="Reference3DType" id="3" name="gf-3__21-90_pub_skel"/>

    
    public List<ModelInstance> childrenList;
    public Reference3D(string name):base(name)
    {

    }

    public bool RenderNew()
    {
        return true;
    }
}



//ReferenceRep和InstanceRep一般成对出现。所以InstanceRep的子节点一般只能是ReferenceRep，而且一般只能有一个。但是可能很多地方都引用他们，所以parent可能不止一个。
class ReferenceRep : ModelInstance
{
    //示例：<ReferenceRep xsi:type="ReferenceRepType" id="4" name="gf-3__21-90_pub_skel" format="TESSELLATED" version="1.2" associatedFile="urn:3DXML:gf-3__21-90_pub_skel.prt.3DRep">
    public RepPart prt;
    public ReferenceRep(string name,string filepath) : base(name)
    {

    }

    public bool RenderNew()
    {
        return true;
    }

}

class InstanceRep : ModelInstance
{
    //<InstanceRep xsi:type="InstanceRepType" id="6" name="Part_InstanceRep">
    //<IsAggregatedBy>3</IsAggregatedBy>
    //<IsInstanceOf>4</IsInstanceOf>
    //</InstanceRep>
    public ReferenceRep child;

    public InstanceRep(string name) : base(name)
    {

    }

    public bool Render()
    {
        return child.RenderNew();
    }
}

class Instance3D : ModelInstance
{
    //<Instance3D xsi:type="Instance3DType" id="17" name="GF-3__21-90_CABLE_PUB">
    //<IsAggregatedBy>1</IsAggregatedBy>
    //<IsInstanceOf>2</IsInstanceOf>
    //<RelativeMatrix>1 0 0 0 1 0 0 0 1 0 0 0</RelativeMatrix>
    //</Instance3D>

    public List<ModelInstance> childrenList;
    public ModelInstance parent;
    public string RelativeMatrix;

    public Instance3D(string name,string relative) : base(name)
    {
        RelativeMatrix = relative;
    }

    public bool Render()
    {
        return true;
    }
    
}