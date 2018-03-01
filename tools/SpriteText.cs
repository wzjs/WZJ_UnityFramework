using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
namespace WZJ_UnityFrameWork
{
    /// <summary>
    /// 用于图文并排，将Text组件替换为此组件后，
    /// 通过富文本 <quad name=xx size=xx width=xx x=xx y=xx>表示一个sprite,
    /// 例如：此处有一张图片:<quad name=xx size=xx width=xx x=xx y=xx>  
    /// name表示资源名，资源必须来自Resources
    /// </summary>
    public class SpriteText : Text
    {
        /// <summary>
        /// 图片池
        /// </summary>
        private readonly List<Image> m_ImagesPool = new List<Image>();

        /// <summary>
        /// 图片的最后一个顶点的索引
        /// </summary>
        private readonly List<int> m_ImagesVertexIndex = new List<int>();

        /// <summary>
        /// 正则取出所需要的属性
        /// </summary>
        private static readonly Regex s_Regex =
              new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) x=([-]?\d*\.?\d+%?) y=([-]?\d*\.?\d+%?)/>", RegexOptions.Singleline);

        float offiX;
        float offiY;
        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            UpdateQuadImage();

        }

        protected void UpdateQuadImage()
        {
            m_ImagesVertexIndex.Clear();
            foreach (Match match in s_Regex.Matches(text))
            {
                var endIndex = match.Index * 4;
                m_ImagesVertexIndex.Add(endIndex);

                m_ImagesPool.RemoveAll(image => image == null);
                if (m_ImagesPool.Count == 0)
                {
                    GetComponentsInChildren<Image>(m_ImagesPool);
                }
                if (m_ImagesVertexIndex.Count > m_ImagesPool.Count)
                {
                    var resources = new DefaultControls.Resources();
                    var go = DefaultControls.CreateImage(resources);
                    go.layer = gameObject.layer;
                    var rt = go.transform as RectTransform;
                    if (rt)
                    {
                        rt.SetParent(rectTransform);
                        rt.localPosition = Vector3.zero;
                        rt.localRotation = Quaternion.identity;
                        rt.localScale = Vector3.one;
                    }
                    m_ImagesPool.Add(go.GetComponent<Image>());
                }

                var spriteName = match.Groups[1].Value;
                var size = float.Parse(match.Groups[2].Value);
                var img = m_ImagesPool[m_ImagesVertexIndex.Count - 1];
                if (img.sprite == null || img.sprite.name != spriteName)
                {
                    img.sprite = Resources.Load<Sprite>(spriteName);
                }
                float x;
                float y;
                if (float.TryParse(match.Groups[4].Value, out x))
                {
                    offiX = x;
                }
                if (float.TryParse(match.Groups[5].Value, out y))
                {
                    offiY = y;
                }

                img.rectTransform.sizeDelta = new Vector2(size, size);
                img.enabled = true;
            }

            for (var i = m_ImagesVertexIndex.Count; i < m_ImagesPool.Count; i++)
            {
                if (m_ImagesPool[i])
                {
                    m_ImagesPool[i].enabled = false;
                }
            }
        }
        [System.Obsolete]
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            var verts = new List<UIVertex>();
            UIVertex uIVertex;
            for (int i = 0; i < toFill.currentVertCount; i++)
            {
                uIVertex = new UIVertex();
                toFill.PopulateUIVertex(ref uIVertex, i);
                verts.Add(uIVertex);
            }

            for (var i = 0; i < m_ImagesVertexIndex.Count; i++)
            {
                var endIndex = m_ImagesVertexIndex[i];
                var rt = m_ImagesPool[i].rectTransform;
                var size = rt.sizeDelta;
                if (endIndex < verts.Count)
                {
                    rt.anchoredPosition = new Vector2(verts[endIndex].position.x + size.x / 2 + offiX, verts[endIndex].position.y - size.y / 2 + offiY);
                    Debug.Log(rt.anchoredPosition);
                }
            }
            var endIndex1 = m_ImagesVertexIndex[0];
            for (int i = 0; i < 4; i++)
            {
                toFill.SetUIVertex(new UIVertex(), endIndex1 + i);
            }
        }
    }
}