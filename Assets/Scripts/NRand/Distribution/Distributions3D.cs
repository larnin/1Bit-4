using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NRand
{
    public static class Rand3D
    {
        public static Vector3 UniformVector3SphereDistribution(IRandomGenerator gen)
        {
            return UniformVector3SphereDistribution(1.0f, gen);
        }

        public static Vector3 UniformVector3SphereDistribution(float radius, IRandomGenerator gen)
        {
            float r = Rand.UniformFloatDistribution(gen);
            r = Mathf.Pow(r, 1 / 3.0f) * radius;
            float yaw = Rand.UniformFloatDistribution(2 * Mathf.PI, gen);
            float pitch = Mathf.Acos(Rand.UniformFloatDistribution(gen) * 2 - 1);

            return new Vector3(Mathf.Cos(yaw) * Mathf.Sin(pitch) * radius, Mathf.Sin(yaw) * Mathf.Sin(pitch) * radius, Mathf.Cos(pitch) * radius);
        }

        //-------------------

        public static Vector3 UniformVector3SphereSurfaceDistribution(IRandomGenerator gen)
        {
            return UniformVector3SphereSurfaceDistribution(1.0f, gen);
        }

        public static Vector3 UniformVector3SphereSurfaceDistribution(float radius, IRandomGenerator gen)
        {
            float yaw = Rand.UniformFloatDistribution(2 * Mathf.PI, gen);
            float pitch = Mathf.Acos(Rand.UniformFloatDistribution(gen) * 2 - 1);

            return new Vector3(Mathf.Cos(yaw) * Mathf.Sin(pitch) * radius, Mathf.Sin(yaw) * Mathf.Sin(pitch) * radius, Mathf.Cos(pitch) * radius);
        }

        //-------------------

        public static Vector3 UniformVector3BoxDistribution(IRandomGenerator gen)
        {
            return UniformVector3BoxDistribution(0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f, gen);
        }

        public static Vector3 UniformVector3BoxDistribution(float max, IRandomGenerator gen)
        {

            return UniformVector3BoxDistribution(0.0f, max, 0.0f, max, 0.0f, max, gen);
        }

        public static Vector3 UniformVector3BoxDistribution(float min, float max, IRandomGenerator gen)
        {
            return UniformVector3BoxDistribution(min, max, min, max, min, max, gen);
        }

        public static Vector3 UniformVector3BoxDistribution(float maxX, float maxY, float maxZ, IRandomGenerator gen)
        {
            return UniformVector3BoxDistribution(0.0f, maxX, 0.0f, maxY, 0.0f, maxZ, gen);
        }

        public static Vector3 UniformVector3BoxDistribution(Bounds bounds, IRandomGenerator gen)
        {
            var min = bounds.min;
            var max = bounds.max;
            return UniformVector3BoxDistribution(min.x, max.x, min.y, max.y, min.z, max.z, gen);
        }

        public static Vector3 UniformVector3BoxDistribution(float minX, float maxX, float minY, float maxY, float minZ, float maxZ, IRandomGenerator gen)
        {
            float x = Rand.UniformFloatDistribution(minX, maxX, gen);
            float y = Rand.UniformFloatDistribution(minY, maxY, gen);
            float z = Rand.UniformFloatDistribution(minZ, maxZ, gen);

            return new Vector3(x, y, z);
        }

        //-------------------

        public static Vector3 UniformVector3BoxSurfaceDistribution(IRandomGenerator gen)
        {
            return UniformVector3BoxSurfaceDistribution(0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f, gen);
        }

        public static Vector3 UniformVector3BoxSurfaceDistribution(float max, IRandomGenerator gen)
        {
            return UniformVector3BoxSurfaceDistribution(0.0f, max, 0.0f, max, 0.0f, max, gen);
        }

        public static Vector3 UniformVector3BoxSurfaceDistribution(float min, float max, IRandomGenerator gen)
        {
            return UniformVector3BoxSurfaceDistribution(min, max, min, max, min, max, gen);
        }

        public static Vector3 UniformVector3BoxSurfaceDistribution(float maxX, float maxY, float maxZ, IRandomGenerator gen)
        {
            return UniformVector3BoxSurfaceDistribution(0.0f, maxX, 0.0f, maxY, 0.0f, maxZ, gen);
        }

        public static Vector3 UniformVector3BoxSurfaceDistribution(Bounds bounds, IRandomGenerator gen)
        {
            var min = bounds.min;
            var max = bounds.max;
            return UniformVector3BoxSurfaceDistribution(min.x, max.x, min.y, max.y, min.z, max.z, gen);
        }

        public static Vector3 UniformVector3BoxSurfaceDistribution(float minX, float maxX, float minY, float maxY, float minZ, float maxZ, IRandomGenerator gen)
        {
            float sizeX = maxX - minX;
            float sizeY = maxY - minY;
            float sizeZ = maxZ - minZ;

            float surfaceXY = sizeX * sizeY;
            float surfaceXZ = sizeX * sizeZ;
            float surfaceYZ = sizeY * sizeZ;

            var value = Rand2D.UniformVector2SquareDistribution((surfaceXY + surfaceXZ + surfaceYZ) * 2, 1, gen);
            if (value.x < surfaceXY)
                return new Vector3(minX + value.x / surfaceXY * sizeX, minY + value.y * sizeY, minZ);
            value.x -= surfaceXY;
            if (value.x < surfaceXY)
                return new Vector3(minX + value.x / surfaceXY * sizeX, minY + value.y * sizeY, minZ + sizeZ);
            value.x -= surfaceXY;
            if (value.x < surfaceXZ)
                return new Vector3(minX + value.x / surfaceXZ * sizeX, minY, minZ + value.y * sizeZ);
            value.x -= surfaceXZ;
            if (value.x < surfaceXZ)
                return new Vector3(minX + value.x / surfaceXZ * sizeX, minY + sizeY, minZ + value.y * sizeZ);
            value.x -= surfaceXZ;
            if (value.x < surfaceYZ)
                return new Vector3(minX, minY + value.x / surfaceYZ * sizeY, minY + value.y * sizeZ);
            value.x -= surfaceYZ;
            return new Vector3(minX + sizeX, minY + value.x / surfaceYZ * sizeY, minY + value.y * sizeZ);
        }
    }
}
