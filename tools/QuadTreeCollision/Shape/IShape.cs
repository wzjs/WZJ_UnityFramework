using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRegularShape
{
    public float xMin { get; }
    public float xMax { get; }
    public float yMin { get; }
    public float yMax { get; }
    public Vector3 center { get; }
}
