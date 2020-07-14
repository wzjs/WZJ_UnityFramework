using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//使用弧度曲线来修改每个物体的坐标
public class RegGrid : UIGrid {
    public AnimationCurve animationCurve;
    protected override void ResetPosition(List<Transform> list)
    {
        maxPerLine = 0;
        mReposition = false;

        // Epic hack: Unparent all children so that we get to control the order in which they are re-added back in
        // EDIT: Turns out this does nothing.
        //for (int i = 0, imax = list.Count; i < imax; ++i)
        //	list[i].parent = null;

        int x = 0;
        int y = 0;
        int maxX = 0;
        int maxY = 0;
        Transform myTrans = transform;

        // Re-add the children in the same order we have them in and position them accordingly
        for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            Transform t = list[i];
            // See above
            //t.parent = myTrans;

            Vector3 pos = t.localPosition;
            float depth = pos.z;

            if (arrangement == Arrangement.CellSnap)
            {
                if (cellWidth > 0) pos.x = Mathf.Round(pos.x / cellWidth) * cellWidth;
                if (cellHeight > 0) pos.y = Mathf.Round(pos.y / cellHeight) * cellHeight;
            }
            else {
                float time = x * 1.0f / (imax - 1);
                if (arrangement == Arrangement.Horizontal)
                {
                    var offset = animationCurve.Evaluate(time) * cellHeight; 
                    pos = new Vector3(cellWidth * x, -cellHeight * y + offset, depth);
                }
                else
                {
                    var offset = animationCurve.Evaluate(time) * cellWidth;
                    pos = new Vector3(cellWidth * y + offset, -cellHeight * x , depth);
                }
            } 

            if (animateSmoothly && Application.isPlaying && (pivot != UIWidget.Pivot.TopLeft || Vector3.SqrMagnitude(t.localPosition - pos) >= 0.0001f))
            {
                SpringPosition sp = SpringPosition.Begin(t.gameObject, pos, 15f);
                sp.updateScrollView = true;
                sp.ignoreTimeScale = true;
            }
            else t.localPosition = pos;

            maxX = Mathf.Max(maxX, x);
            maxY = Mathf.Max(maxY, y);

            if (++x >= maxPerLine && maxPerLine > 0)
            {
                x = 0;
                ++y;
            }
        }

        // Apply the origin offset
        if (pivot != UIWidget.Pivot.TopLeft)
        {
            Vector2 po = NGUIMath.GetPivotOffset(pivot);

            float fx, fy;

            if (arrangement == Arrangement.Horizontal)
            {
                fx = Mathf.Lerp(0f, maxX * cellWidth, po.x);
                fy = Mathf.Lerp(-maxY * cellHeight, 0f, po.y);
            }
            else
            {
                fx = Mathf.Lerp(0f, maxY * cellWidth, po.x);
                fy = Mathf.Lerp(-maxX * cellHeight, 0f, po.y);
            }

            foreach (var t in list)
            {
                var sp = t.GetComponent<SpringPosition>();

                if (sp != null)
                {
                    sp.enabled = false;
                    sp.target.x -= fx;
                    sp.target.y -= fy;
                    sp.enabled = true;
                }
                else
                {
                    Vector3 pos = t.localPosition;
                    pos.x -= fx;
                    pos.y -= fy;
                    t.localPosition = pos;
                }
            }
        }
    }
}
