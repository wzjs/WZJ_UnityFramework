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
        index = 2;
        length = 3;

        UIVertex uIVertex = new UIVertex();
        if (text.Length >= index + length)
        {
            toFill.PopulateUIVertex(ref uIVertex, index * 4);
            float leftwidth = uIVertex.position.x;
            Debug.Log("left:" + leftwidth);
            int a = index * 4 + length * 4 - 4;
            toFill.PopulateUIVertex(ref uIVertex, a);
            float rightwidth = uIVertex.position.x;
            Debug.Log("right:" + rightwidth);

            toFill.PopulateUIVertex(ref uIVertex, index * 4 + 1);
            float allWidht = (uIVertex.position.x - leftwidth) * (length - 1);
            Debug.Log("allwidht:" + allWidht);
            for (int i = 0; i < length * 4; i++)
            {
                toFill.PopulateUIVertex(ref uIVertex, index * 4 + i);
                float f = (uIVertex.position.x - leftwidth) / allWidht;
                Debug.Log(f);
                uIVertex.color = Color32.Lerp(Color.red, Color.blue, f);
                toFill.SetUIVertex(uIVertex, index * 4 + i);
            }
        }

    }

    public override void Rebuild(CanvasUpdate update)
    {
        base.Rebuild(update);
        
    }
}
