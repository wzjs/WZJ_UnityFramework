using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class RectCollisioner : Collisioner
{
    public int Width;
    public int Height;
    
    public override IRegularShape GetShape()
    {
        return new TRect(transform.localPosition.x - Width/2, transform.localPosition.y - Height / 2, Width,Height);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        IRegularShape shape = GetShape();
        float width = (shape.xMax - shape.xMin);
        float height = (shape.yMax - shape.yMin);

        Handles.DrawWireCube(shape.center, new Vector3(width, height));
    }
#endif

}
