using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace WZJ_UnityFrameWork
{
    /// <summary>
    /// 从FBX中导出动画
    /// 具体：选中一个或多个文件夹，运行脚本后自动遍历所有文件（包括子文件）,然后自动在选中文件夹下创建anims文件夹，然后动画都存放在anims文件夹中
    /// 需要一个文本记录clip信息，文本名称必须得和模型名称相同。
    /// </summary>
    public class AnimClipExporter
    {

        [MenuItem("Tools/ExportAnimClip")]
        static void ImportAsset()
        {

            //string[] all = AssetDatabase.GetAllAssetPaths();
            UnityEngine.Object[] all = Selection.objects;
            foreach (UnityEngine.Object str1 in all)
            {
                DefaultAsset d = str1 as DefaultAsset;
                if (d == null)
                {
                    Debug.Log("您选中的不是文件夹");
                    return;
                }
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                string[] allfbx = Directory.GetFiles(path, @"*.fbx", SearchOption.AllDirectories);
                foreach (string str in allfbx)
                {
                    AssetImporter import = ModelImporter.GetAtPath(str);
                    var strarr = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(import.assetPath) + " t:textAsset");

                    if (strarr.Length != 1)
                    {
                        Debug.LogError("文本文件多于1或者少于1");
                        return;
                    }
                    string fileAnim = AssetDatabase.GUIDToAssetPath(strarr[0]);

                    StreamReader file = new StreamReader(fileAnim);

                    string sAnimList = file.ReadToEnd();
                    file.Close();
                    System.Collections.ArrayList List = new ArrayList();
                    ParseAnimFile(sAnimList, ref List);
                    var t = (ModelImporterClipAnimation[])
                        List.ToArray(typeof(ModelImporterClipAnimation));

                    ModelImporter modelImporter = import as ModelImporter;
                    modelImporter.clipAnimations = t;

                    modelImporter.SaveAndReimport();

                    //EditorUtility.SetDirty(modelImporter);
                    //AssetDatabase.SaveAssets();

                    //AssetDatabase.ImportAsset(str);
                    string dir = Directory.GetParent(Directory.GetParent(import.assetPath).FullName).FullName + "/Animation";
                    Debug.Log(dir);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    dir = dir.Remove(0, dir.IndexOf("Asset"));

                    var allObj = AssetDatabase.LoadAllAssetsAtPath(import.assetPath);



                    foreach (var item in allObj)
                    {
                        AnimationClip c1 = item as AnimationClip;
                        try
                        {
                            if (c1 != null)
                            {

                                //var a1 = t.First<ModelImporterClipAnimation>((a) =>
                                //{
                                //	if (a.name == c1.name)
                                //		return true;
                                //	else
                                //		return false;
                                //});

                                //AnimationClipSettings t1 = AnimationUtility.GetAnimationClipSettings(c1);
                                //t1.loopTime = a1.loopTime;
                                //AnimationUtility.SetAnimationClipSettings(c1, t1);


                                AnimationClip newClip = new AnimationClip();
                                EditorUtility.CopySerialized(c1, newClip);





                                AssetDatabase.CreateAsset(newClip, dir + "/" + c1.name + ".anim");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(c1.name);
                        }
                    }
                }
            }
            EditorUtility.DisplayDialog("已完成", "导出完毕", "完成");

        }

        static void ParseAnimFile(string sAnimList, ref System.Collections.ArrayList List)
        {
            Regex regexString = new Regex(" *(?<firstFrame>[0-9]+) *- *(?<lastFrame>[0-9]+) *(?<loop>(loop|noloop| )) *(?<name>[^\r^\n]*[^\r^\n^ ])",
                                          RegexOptions.Compiled | RegexOptions.ExplicitCapture);

            Match match = regexString.Match(sAnimList, 0);
            while (match.Success)
            {
                ModelImporterClipAnimation clip = new ModelImporterClipAnimation();

                if (match.Groups["firstFrame"].Success)
                {
                    clip.firstFrame = System.Convert.ToInt32(match.Groups["firstFrame"].Value, 10);
                }
                if (match.Groups["lastFrame"].Success)
                {
                    clip.lastFrame = System.Convert.ToInt32(match.Groups["lastFrame"].Value, 10);
                }
                if (match.Groups["loop"].Success)
                {
                    clip.loopTime = match.Groups["loop"].Value == "loop";
                }
                if (match.Groups["name"].Success)
                {
                    clip.name = match.Groups["name"].Value;
                }

                List.Add(clip);

                match = regexString.Match(sAnimList, match.Index + match.Length);
            }
        }
    }

}
