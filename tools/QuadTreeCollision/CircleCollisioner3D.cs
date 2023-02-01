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

public class CircleCollisioner3D : Collisioner
{
    public float Raduis;
    public override IRegularShape GetShape()
    {
        return new Circle(new Vector3(transform.localPosition.x, transform.localPosition.z), Raduis);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        var shape = GetShape();
        Handles.DrawWireDisc(new Vector3(shape.center.x,0,shape.center.y), Vector3.up, Raduis);
    }
#endif
}
