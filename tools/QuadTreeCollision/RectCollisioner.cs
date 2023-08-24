#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class RectCollisioner : Collisioner
{
    public float Width;
    public float Height;
    public Vector2 Offset;

    public override IRegularShape GetShape()
    {
        return new TRect(transform.position.x - Width/2 + Offset.x, transform.position.y - Height / 2 + Offset.y, Width,Height);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(QuadTreeCollisionSystem.GetInstance().IsDebug)
        {
            Handles.color = Color.black;
            IRegularShape shape = GetShape();
            float width = (shape.xMax - shape.xMin);
            float height = (shape.yMax - shape.yMin);
            Handles.DrawWireCube(shape.center, new Vector3(width, height));
        }
        
    }
#endif

}
