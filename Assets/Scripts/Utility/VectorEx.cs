using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class VectorEx
{
    public static float MagnitudeXZ(this Vector3 vect)
    {
        return Mathf.Sqrt(vect.SqrMagnitudeXZ());
    }

    public static float SqrMagnitudeXZ(this Vector3 vect)
    {
        return vect.x * vect.x + vect.z * vect.z;
    }

    public static Rect Encapsulate(this Rect rect, Vector2 point)
    {
        var min = rect.min;
        var max = rect.max;

        if (point.x < min.x)
            min.x = point.x;
        if (point.x > max.x)
            max.x = point.x;
        if (point.y < min.y)
            min.y = point.y;
        if (point.y > max.y)
            max.y = point.y;

        var size = max - min;

        rect.Set(min.x, min.y, size.x, size.y);

        return rect;
    }
}

