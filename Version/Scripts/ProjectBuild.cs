using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

class ProjectBuild : Editor
{

    //在这里找出你当前工程所有的场景文件，假设你只想把部分的scene文件打包 那么这里可以写你的条件判断 总之返回一个字符串数组。
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();

        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    //shell脚本直接调用这个静态方法
    static void BuildForIPhone()
    {
        var arg = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < arg.Length; i++)
        {
            Debug.Log(arg[i]);
        }

        if (arg.Length < 2)
        {
            Debug.LogError("please add argu to your commandline...");
            return;
        }
        if (arg[arg.Length - 2] == "dis")
        {
            Debug.Log("start distrbuition package...");
            PlayerSettings.applicationIdentifier = "com.xindong.thelastcrown";
            BuildPipeline.BuildPlayer(GetBuildScenes(), "/Users/dongliang/Version_Branch/dis/" + arg[arg.Length - 3], BuildTarget.iOS, BuildOptions.None);
        }
        else
        {
            Debug.Log("start development package...");
            PlayerSettings.applicationIdentifier = "com.xindong.lastcrown";

            BuildPipeline.BuildPlayer(GetBuildScenes(), "/Users/dongliang/Version_Branch/dev/" + arg[arg.Length - 3], BuildTarget.iOS, BuildOptions.Development);
        }

    }
}
