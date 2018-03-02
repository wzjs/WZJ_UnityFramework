using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
namespace WZJ_UnityFrameWork
{
    /// <summary>
    /// 从FBX中导出动画
    /// 具体：选中一个或多个文件夹，运行脚本后自动遍历所有文件（包括子文件）,然后自动在选中文件夹下创建anims文件夹，然后动画都存放在anims文件夹中
    /// </summary>
    public class AnimClipExporter
    {

        [MenuItem("Tools/ExportAnimClip")]
        public static void ExportAnimClip()
        {
            var all = Selection.objects;
            try
            {
                foreach (var item in all)
                {

                    if (item.GetType() != typeof(DefaultAsset))
                    {
                        return;
                    }
                    string path = AssetDatabase.GetAssetPath(item);
                    Debug.Log(path);
                    string[] allName = Directory.GetFiles(path, "*.FBX", SearchOption.AllDirectories);

                    string savepath = path + "/anims/";
                    if (!Directory.Exists(savepath))
                    {
                        Directory.CreateDirectory(savepath);
                    }
                    int index = 0;

                    foreach (string name in allName)
                    {
                        index++;

                        EditorUtility.DisplayProgressBar("导出动画", "导出" + name, index / allName.Length * 1.0f);

                        Object[] clips = AssetDatabase.LoadAllAssetsAtPath(name);

                        foreach (var clip in clips)
                        {
                            AnimationClip animationClip = clip as AnimationClip;
                            if (animationClip != null)
                            {
                                if (animationClip.name[0] == '_') continue;
                                AnimationClip animation = new AnimationClip();
                                EditorUtility.CopySerialized(animationClip, animation);


                                AssetDatabase.CreateAsset(animation, savepath + clip.name + ".anim");
                            }
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                Debug.Log("error");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

        }
    }

}
