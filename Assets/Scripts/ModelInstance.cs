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

class ModelInstance
{
    RepPart prt;
    string transformString;


    public ModelInstance(RepPart p,string s)
    {
        prt = p;
        transformString = s;
    }

    public bool Render()
    {
        return prt.renderNewInstance(transformString);
    }

}