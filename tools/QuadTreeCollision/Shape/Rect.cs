using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TRect : IRegularShape
{
    private UnityEngine.Rect _core;

    public float xMin => _core.xMin;

    public float xMax => _core.xMax;

    public float yMin => _core.yMin;

    public float yMax => _core.yMax;

    public Vector3 center => _core.center;

    public TRect(UnityEngine.Rect rect)
    {
        _core = rect;
    }

    public TRect(float x, float y, float width, float height)
    {
        _core = new UnityEngine.Rect(x, y, width, height);
    }

    public TRect(Vector3 pos, Vector3 size)
    {
        _core = new UnityEngine.Rect(pos, size);
    }
}
