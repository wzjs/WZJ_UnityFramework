using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 切割精灵后有多资源情况下，导出多张图片的工具。
/// </summary>
public class SpritesExporter : MonoBehaviour {

	[MenuItem("Tools/SpritesExporter")]
    public static void ExportSprites()
    {
        string savePath =  EditorUtility.OpenFolderPanel("保存路径位置", "D://", "");
        if (savePath == "") return;

        int count = 0;
        int index = 0;
        foreach (var item in Selection.objects)
        {
            index++;
            EditorUtility.DisplayProgressBar("导出图片", "第" + index + "张导出...", index / Selection.objects.Length * 1.0f);

            if (item.GetType() == typeof(Texture2D))
            {
                string assetPath = AssetDatabase.GetAssetPath(item);
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var sprite in objs)
                {
                    Sprite s = sprite as Sprite;
                    if (s != null)
                    {
                        Texture2D texture2D = new Texture2D((int)s.rect.width, (int)s.rect.height, s.texture.format, false);
                        texture2D.SetPixels(s.texture.GetPixels((int)s.rect.xMin, (int)s.rect.yMin, (int)s.rect.width, (int)s.rect.height));
                        texture2D.Apply();

                        System.IO.File.WriteAllBytes(savePath + s.name+".png", texture2D.EncodeToPNG());
                        Debug.Log("success:" + s.name);
                        count++;
                    }
                }
            }
            else
            {
                Debug.Log(item.name + "不是Texture2D");
            }  
        }
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("hint", "一共导出" + count + "个图片", "确定");
    }
}
