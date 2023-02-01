using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Circle : IRegularShape
{
    public Vector3 Center;
    public float Radius;

    public Circle(Vector3 center, float radius)
    {
        this.Center = center;
        this.Radius = radius;
    }

    public float xMin => Center.x - Radius;

    public float xMax => Center.x + Radius;

    public float yMin => Center.y - Radius;

    public float yMax => Center.y + Radius;

    public Vector3 center => Center;
}
