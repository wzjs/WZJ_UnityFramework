#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CircleCollisioner : Collisioner
{
    public int Raduis;
    public override IRegularShape GetShape()
    {
        return new Circle(transform.localPosition, Raduis);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (QuadTreeCollisionSystem.GetInstance().IsDebug)
        {
            Handles.color = Color.black;
            var shape = GetShape();
            Handles.DrawWireDisc(shape.center, Vector3.forward, Raduis);
        }
    }
#endif
}
