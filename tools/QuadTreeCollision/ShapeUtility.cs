using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class ShapeUtility
{
    public static bool IsContain(IRegularShape container, IRegularShape containee) {
        return container.xMin <= containee.xMin && container.yMin <= containee.yMin 
            && container.xMax >= containee.xMax && container.yMax >= containee.yMax;
    }

    public static bool IsIntersect(TRect rect,TRect rect1)
    {
        return rect.xMax > rect1.xMin && rect.xMin < rect1.xMax && rect.yMax > rect1.yMin && rect.yMin < rect1.yMax;
    }

    public static bool IsIntersect(TRect rect, Circle circle)
    {
        float x = 0;
        float y = 0;
        if(circle.center.x >= rect.xMin && circle.center.x <= rect.xMax)
        {
            x = circle.center.x;
        }
        else if(circle.center.x < rect.xMin)
        {
            x = rect.xMin;
        }
        else if(circle.center.x > rect.xMax)
        {
            x = rect.xMax;
        }
        else { Debug.LogError("Intersect error"); }

        if (circle.center.y >= rect.yMin && circle.center.y <= rect.yMax)
        {
            y = circle.center.y;
        }
        else if (circle.center.y < rect.yMin)
        {
            y = rect.yMin;
        }
        else if (circle.center.y > rect.yMax)
        {
            y = rect.yMax;
        }
        else { Debug.LogError("Intersect error"); }
        var closestPoint = new Vector3(x, y);

        return Vector3.Distance(closestPoint,circle.Center) <= circle.Radius || IsContain(rect,circle);
    }

    public static bool IsIntersect(Circle circle,Circle circle1)
    {
        return Vector3.Distance(circle.Center,circle1.Center) <= (circle.Radius + circle1.Radius);
    }

    public static bool IsIntersect(IRegularShape shape,IRegularShape shape1)
    {
        var isCircle = shape.GetType() == typeof(Circle);
        var isCircle1 = shape1.GetType() == typeof(Circle);
        if(isCircle && isCircle1)
        {
            var circle = (Circle)shape;
            var circle1 = (Circle)shape1;

            return IsIntersect(circle, circle1);
        }else if(!isCircle && !isCircle1)
        {
            var rect = (TRect)shape;
            var rect1 = (TRect)shape1;
            return IsIntersect(rect, rect1);
        }
        else
        {
            TRect rect;
            Circle circle;
            rect = isCircle ? (TRect)shape1 : (TRect)shape;
            circle = isCircle ? (Circle)shape : (Circle)shape1;
            return IsIntersect(rect, circle);
        }
    }
}
