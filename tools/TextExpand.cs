using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextExpand : Text {

    public int index;
    public int length;
    
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        index = 1;
        length = 30;
        Debug.Log(length);
        UIVertex uIVertex = new UIVertex();
        if (text.Length >= index + length)
        {
            toFill.PopulateUIVertex(ref uIVertex, index * 4);
            float leftwidth = uIVertex.position.x;
            float firstHeight = uIVertex.position.y;
            Debug.Log("left:" + leftwidth);
            toFill.PopulateUIVertex(ref uIVertex, index * 4 + 1);
            float secondWdith = uIVertex.position.x;
            toFill.PopulateUIVertex(ref uIVertex, index * 4 + 2);
            float secondHight = uIVertex.position.y;
            Debug.Log("right:" + secondWdith);
            float off = secondWdith - leftwidth;
            float offHeight_2 = (firstHeight - secondHight) / 2;
            toFill.PopulateUIVertex(ref uIVertex, index * 4 + 1);
            float allWidht = (uIVertex.position.x - leftwidth) * (length - 1);
            Debug.Log("allwidht1:" + allWidht);
            List<UIVertex> uiver = new List<UIVertex>();
            for (int i = 0; i < length * 4; i++)
            {
                toFill.PopulateUIVertex(ref uIVertex, index * 4 + i);
                uiver.Add(uIVertex);
            }
            float x = 0;
            //因为换行会使颜色显示不正常，所以将指定变色的顶点数据都改成按一行的形式
            for (int i = 0; i < uiver.Count; i+=4)
            {
                if(uiver[i].position.y < firstHeight - offHeight_2)
                {
                    x = x + 7;

                    UIVertex u = uiver[i];
                    u.position = new Vector3(x, 0);
                    uiver[i] = u;

                    u = uiver[i+1];
                    u.position = new Vector3(x + off, 0);
                    uiver[i + 1] = u;

                    u = uiver[i + 2];
                    u.position = new Vector3(x + off, 0);
                    uiver[i + 2] = u;

                    u = uiver[i + 3];
                    u.position = new Vector3(x, 0);
                    uiver[i + 3] = u;

                    x = x + off;
                }
                else
                {
                    x = uiver[i].position.x;
                }

            }
            for (int i = 0; i < length * 4; i++)
            {
                toFill.PopulateUIVertex(ref uIVertex, index * 4 + i);
                float f = (uiver[i].position.x - leftwidth) / allWidht;
                Debug.Log(f);
                uIVertex.color = Color32.Lerp(Color.red, Color.blue, f);
                toFill.SetUIVertex(uIVertex, index * 4 + i);
            }
        }

    }
}
