using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;



/// <summary>
/// GeneratorBehaviour类说明：
/// 此类是unity3D中的Gameobject：ModelGenerator的行为类。此类主要功能为开一个后台进程进行模型加载（防止前台运行加载程序时导致的场景卡顿），并将模型显示在场景中。
/// /// 作者：zy-zhao17@mails.tsinghua.edu.cn
/// </summary>

public class GeneratorBehaviour : MonoBehaviour
{
    private BackgroundWorker bgWorker;

    XmlModel mainModel = null;

    GameObject thisobject;

    private long starttime;

    public void bgWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        Debug.Log("background worker started with name "+e.Argument);//将会打出调用这个后台进程时给出的参数，例如：helloworld

        mainModel = new XmlModel("Assets\\3dxml\\gf-3-2-0_cable_pub.asm.3dxml");



    }

    public void bgWorker_ProgessChanged(object sender, ProgressChangedEventArgs e)
    {
        string state = (string)e.UserState;  //接收ReportProgress方法传递过来的userState
        Debug.Log(state + e.ProgressPercentage);
    }



    public void bgWorker_WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Error != null)
        {
            Debug.Log(e.Error.ToString());
            return;
        }
        if (!e.Cancelled)
            Debug.Log("处理完毕!");
        else
            Debug.Log("处理终止!");

    }



    //private void btnStop_Click(object sender, EventArgs e)
    //{
    //    bgWorker.CancelAsync();
    //}



    // Start is called before the first frame update
    void Start()
    {
        bgWorker = new BackgroundWorker();
        bgWorker.WorkerReportsProgress = true;
        bgWorker.WorkerSupportsCancellation = true;
        bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
        bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgessChanged);
        bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_WorkerCompleted);
        Debug.Log("开启后台进程");
        bgWorker.RunWorkerAsync("helloworld");

        starttime = -1;

    }


    // Update is called once per frame
    void Update()
    {
        if ((mainModel != null) && mainModel.loadFinished && (!mainModel.haveRendered))
        {
            mainModel.Render(gameObject);
        }

    }

}
