using UnityEngine;
#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Collections.Generic;
using System.IO;
#endif

public class XCodeProjectMod : MonoBehaviour
{
#if UNITY_IOS
    private const string SETTING_DATA_PATH = "Assets/Editor/Setting/XcodeProjectSetting.asset";
    [PostProcessBuild]
    private static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget != BuildTarget.iOS)
            return;
        PBXProject pbxProject = null;
        XcodeProjectSetting setting = null;
        string pbxProjPath = PBXProject.GetPBXProjectPath(buildPath);
        string targetGuid = null;
        Debug.Log("开始设置.XCodeProj");

        setting = AssetDatabase.LoadAssetAtPath<XcodeProjectSetting>(SETTING_DATA_PATH);
        pbxProject = new PBXProject();
        pbxProject.ReadFromString(File.ReadAllText(pbxProjPath));
        targetGuid = pbxProject.TargetGuidByName(PBXProject.GetUnityTargetName());

        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.ENABLE_BITCODE_KEY, setting.EnableBitCode ? "YES" : "NO");
        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.DEVELOPMENT_TEAM, setting.DevelopmentTeam);
        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.GCC_ENABLE_CPP_EXCEPTIONS, setting.EnableCppEcceptions ? "YES" : "NO");
        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.GCC_ENABLE_CPP_RTTI, setting.EnableCppRtti ? "YES" : "NO");
        pbxProject.SetBuildProperty(targetGuid, XcodeProjectSetting.GCC_ENABLE_OBJC_EXCEPTIONS, setting.EnableObjcExceptions ? "YES" : "NO");

      //  if (!string.IsNullOrEmpty(setting.CopyDirectoryPath))
        //    DirectoryProcessor.CopyAndAddBuildToXcode(pbxProject, targetGuid, setting.CopyDirectoryPath, buildPath, "");

        for (int i = 0; i < setting.CopyDirectoryPath.Count; i++)
        {
            DirectoryProcessor.CopyAndAddBuildToXcode(pbxProject, targetGuid, setting.CopyDirectoryPath[i], buildPath, "");
        }

        //编译器标记（Compiler flags）
        foreach (XcodeProjectSetting.CompilerFlagsSet compilerFlagsSet in setting.CompilerFlagsSetList)
        {
            foreach (string targetPath in compilerFlagsSet.TargetPathList)
            {
                if (!pbxProject.ContainsFileByProjectPath(targetPath))
                    continue;
                string fileGuid = pbxProject.FindFileGuidByProjectPath(targetPath);
                List<string> flagsList = pbxProject.GetCompileFlagsForFile(targetGuid, fileGuid);
                flagsList.Add(compilerFlagsSet.Flags);
                pbxProject.SetCompileFlagsForFile(targetGuid, fileGuid, flagsList);
            }
        }

        var codesign = Debug.isDebugBuild ? "iPhone Developer: Liang Dong (62ZU2QZX6V)" : "iPhone Distribution: X.D. Network Inc. (NTC4BJ542G)";
        var provision = Debug.isDebugBuild ? "XDMagicp" : "TheLastCrown_AppStore";
        var bundleName = Debug.isDebugBuild ? "com.xindong.lastcrown" : "com.xindong.thelastcrown";
        var produceName = Debug.isDebugBuild ? "lastcrown" : "thelastcrown";
        var teamId = Debug.isDebugBuild ? "XF28F2YCJQ" : "NTC4BJ542G";

        pbxProject.SetBuildProperty(targetGuid, "CODE_SIGN_IDENTITY", codesign);
        pbxProject.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", provision);
        pbxProject.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", teamId);
        pbxProject.AddBuildProperty(targetGuid, "PRODUCT_BUNDLE_IDENTIFIER", bundleName);
        pbxProject.AddBuildProperty(targetGuid, "CODE_SIGN_STYLE", "Manual");
        pbxProject.SetBuildProperty(targetGuid, "PRODUCT_NAME", produceName);

        DirectoryProcessor.CopyAndReplace("/Users/dongliang/Perforce/lizheng_dongliangdeMac-mini_2302/CopyiOS/Unity-iPhone",buildPath + "/Unity-iPhone");

        //引用内部框架
        foreach (string framework in setting.FrameworkList)
        {
            pbxProject.AddFrameworkToProject(targetGuid, framework, false);
        }

        //引用.tbd文件
        foreach (string tbd in setting.TbdList)
        {
            pbxProject.AddFileToBuild(targetGuid, pbxProject.AddFile("usr/lib/" + tbd, "Frameworks/" + tbd, PBXSourceTree.Sdk));
        }

        //设置OTHER_LDFLAGS
        pbxProject.UpdateBuildProperty(targetGuid, XcodeProjectSetting.LINKER_FLAG_KEY, setting.LinkerFlagArray, null);
        //设置Framework Search Paths
        pbxProject.UpdateBuildProperty(targetGuid, XcodeProjectSetting.FRAMEWORK_SEARCH_PATHS_KEY, setting.FrameworkSearchPathArray, null);
        File.WriteAllText(pbxProjPath, pbxProject.WriteToString());

        //已经存在的文件，拷贝替换
        foreach (XcodeProjectSetting.CopeFiles file in setting.CopeFilesList)
        {
            File.Copy(Application.dataPath + file.sourcePath, buildPath + file.copyPath, true);
        }

        //File.Copy(Application.dataPath + "/Editor/XCodeAPI/UnityAppController.h", buildPath + "/Classes/UnityAppController.h", true);
        //File.Copy(Application.dataPath + "/Editor/XCodeAPI/UnityAppController.mm", buildPath + "/Classes/UnityAppController.mm", true);

        //设置Plist
        InfoPlistProcessor.SetInfoPlist(buildPath, setting);
    }
#endif
}
