using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;


public class CustomMenuBehaviour : MonoBehaviour
{



    [MenuItem("AR装配/导入模型")]
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

        if (OpenFileDialog.GetOpenFileName(pth))
        {
            string filepath = pth.file;//选择的文件路径;  
            //Debug.Log(filepath);
            XmlModel x = new XmlModel(filepath);
            GameObject go = GameObject.Find("ModelGenerator");
            if (go == null) go = new GameObject("ModelGenerator");
            x.Render(go);
        }
    }

    [MenuItem("AR装配/清空模型")]
    public static void CustomSubMenu2()
    {
        try
        {
            Directory.Delete("Assets\\cached\\", true);
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