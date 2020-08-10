using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EditorUtil
{
    public static void ExecuteProcess(string filePath, string command, int seconds = 0)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            UnityEngine.Debug.LogError(filePath + " 找不到");
            return;
        }
        Process process = new Process();//创建进程对象
        //process.StartInfo.WorkingDirectory = workPath;//文件目录，可以不设置，在FileName中带上路径
        process.StartInfo.FileName = filePath;//文件名
        process.StartInfo.Arguments = command;//参数（会传入到如exe的Main()函数或者bat的%1 %2）
        process.StartInfo.CreateNoWindow = true;//启动该进程而不创建包含它的新窗口
        process.StartInfo.RedirectStandardOutput = false;//不重定向输出
        try
        {
            if (process.Start())
            {
                if (seconds == 0)
                {
                    process.WaitForExit(); //无限等待进程结束
                }
                else
                {
                    process.WaitForExit(seconds); //在指定的毫秒数内等待关联进程退出
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
        finally
        {
            process.Close();
        }
    }
}

