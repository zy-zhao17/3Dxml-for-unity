using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;


public class CustomMenuBehaviour : MonoBehaviour
{



    [MenuItem("AR装配/导入模型并生成XML默认模板")]
    public static void CustomSubMenu1()
    {
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
        pth.filter = "支持的模型文件(*.3DXML)\0*.3DXML";

        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);  // default path  
        pth.title = "导入模型";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (!OpenFileDialog.GetOpenFileName(pth)) return;
        
        string filepath = pth.file;//选择的文件路径;  
                                   //Debug.Log(filepath);
        XmlModel x = new XmlModel(filepath);
        DestroyImmediate(GameObject.Find("ModelGenerator"));
        x.Render(new GameObject("ModelGenerator"));



        //打出一行字：模板生成在。。。.xml，请手动设置动画参数，或使用第二个选项自动设置参数。






    }

    [MenuItem("AR装配/自动填入XML动画参数（未实现）")]
    public static void CustomSubMenu2()
    {

    }

    [MenuItem("AR装配/载入已编辑好的XML动画指令序列")]
    public static void CustomSubMenu3()
    {
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
        pth.filter = "动画模板文件(*.XML)\0*.XML";

        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);  // default path  
        pth.title = "载入模板";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (!OpenFileDialog.GetOpenFileName(pth)) return;

        string filepath = pth.file;//选择的文件路径;  


        string[] strlis = File.ReadAllLines(filepath, Encoding.UTF8);
        File.WriteAllLines("XmlAnimation\\Animation.xml", strlis, Encoding.UTF8);
        Debug.Log("成功载入动画指令序列！");
    }

    [MenuItem("AR装配/清空模型")]
    public static void CustomSubMenu4()
    {
        try
        {
            Directory.Delete("cached\\", true);
        }
        catch (IOException) { }

        DestroyImmediate(GameObject.Find("ModelGenerator"));

    }

}




[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class FileDlg
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileDlg : FileDlg
{

}
public class OpenFileDialog
{
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileDlg ofd);
}