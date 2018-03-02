using UnityEngine;
using UnityEditor;

namespace WZJ_UnityFrameWork
{
    /// <summary>
    /// 制作自定义字体
    /// targetFont：新建一个Custom Font
    /// fntData:用BMFont制作,自行了解BMFont
    /// fontMaterial:新建材质,用Sprite/Default，否则显示不了
    /// fontTexture:了解BMFont
    /// </summary>
    public class BMFontEditor : EditorWindow
    {
        [MenuItem("Tools/BMFont Maker")]
        static public void OpenBMFontMaker()
        {
            EditorWindow.GetWindow<BMFontEditor>(false, "BMFont Maker", true).Show();
        }

        [SerializeField]
        private Font targetFont;
        [SerializeField]
        private TextAsset fntData;
        [SerializeField]
        private Material fontMaterial;
        [SerializeField]
        private Texture2D fontTexture;

        private BMFont bmFont = new BMFont();

        public BMFontEditor()
        {
        }

        void OnGUI()
        {
            targetFont = EditorGUILayout.ObjectField("Target Font", targetFont, typeof(Font), false) as Font;
            fntData = EditorGUILayout.ObjectField("Fnt Data", fntData, typeof(TextAsset), false) as TextAsset;
            fontMaterial = EditorGUILayout.ObjectField("Font Material", fontMaterial, typeof(Material), false) as Material;
            fontTexture = EditorGUILayout.ObjectField("Font Texture", fontTexture, typeof(Texture2D), false) as Texture2D;

            if (GUILayout.Button("Create BMFont"))
            {
                BMFontReader.Load(bmFont, fntData.name, fntData.bytes); // 借用NGUI封装的读取类
                CharacterInfo[] characterInfo = new CharacterInfo[bmFont.glyphs.Count];
                for (int i = 0; i < bmFont.glyphs.Count; i++)
                {
                    BMGlyph bmInfo = bmFont.glyphs[i];
                    CharacterInfo info = new CharacterInfo();
                    info.index = bmInfo.index;
                    info.uv.x = (float)bmInfo.x / (float)bmFont.texWidth;
                    info.uv.y = 1 - (float)bmInfo.y / (float)bmFont.texHeight;
                    info.uv.width = (float)bmInfo.width / (float)bmFont.texWidth;
                    info.uv.height = -1f * (float)bmInfo.height / (float)bmFont.texHeight;
                    info.vert.x = 0;
                    info.vert.y = -(float)bmInfo.height;
                    info.vert.width = (float)bmInfo.width;
                    info.vert.height = (float)bmInfo.height;
                    info.width = (float)bmInfo.advance;
                    characterInfo[i] = info;
                }
                targetFont.characterInfo = characterInfo;
                if (fontMaterial)
                {
                    fontMaterial.mainTexture = fontTexture;
                }
                targetFont.material = fontMaterial;
                fontMaterial.shader = Shader.Find("UI/Default");//这一行很关键，如果用standard的shader，放到Android手机上，第一次加载会很慢

                Debug.Log("create font <" + targetFont.name + "> success");
                Close();
            }
        }
    }
}
