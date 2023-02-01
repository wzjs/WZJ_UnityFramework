#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class RectCollisioner3D : Collisioner
{
    public int Width;
    public int Height;
    
    public override IRegularShape GetShape()
    {//-0.5 -3.5
        return new TRect(transform.localPosition.x - 1.0f*Width/2, transform.localPosition.z - 1.0f * Height / 2, Width,Height);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        IRegularShape shape = GetShape();
        float width = (shape.xMax - shape.xMin);
        float height = (shape.yMax - shape.yMin);
        Handles.DrawWireCube(new Vector3(shape.center.x,0,shape.center.y), new Vector3(width, 0,height));
    }
#endif
}
