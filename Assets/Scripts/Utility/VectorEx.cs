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
}

