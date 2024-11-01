using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NRand
{
    public static class Rand2D
    {
        public static Vector2 UniformVector2CircleDistribution(IRandomGenerator gen)
        {
            return UniformVector2CircleDistribution(1.0f, gen);
        }

        public static Vector2 UniformVector2CircleDistribution(float radius, IRandomGenerator gen)
        {
            float r = Rand.UniformFloatDistribution(gen);
            r = Mathf.Sqrt(r) * radius;
            float angle = Rand.UniformFloatDistribution(2 * Mathf.PI, gen);
            return new Vector3(r * Mathf.Cos(angle), r * Mathf.Sin(angle));
        }

        //-------------------

        public static Vector2 UniformVector2CircleSurfaceDistribution(IRandomGenerator gen)
        {
            return UniformVector2CircleSurfaceDistribution(1.0f, gen);
        }

        public static Vector2 UniformVector2CircleSurfaceDistribution(float radius, IRandomGenerator gen)
        {
            float angle = Rand.UniformFloatDistribution(2 * Mathf.PI, gen);
            return new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
        }

        //-------------------

        public static Vector2 UniformVector2SquareDistribution(IRandomGenerator gen)
        {
            return UniformVector2SquareDistribution(0.0f, 1.0f, 0.0f, 1.0f, gen);
        }

        public static Vector2 UniformVector2SquareDistribution(float max, IRandomGenerator gen)
        {
            return UniformVector2SquareDistribution(0.0f, max, 0.0f, max, gen);
        }

        public static Vector2 UniformVector2SquareDistribution(float min, float max, IRandomGenerator gen)
        {
            return UniformVector2SquareDistribution(min, max, min, max, gen);
        }

        public static Vector2 UniformVector2SquareDistribution(Rect rect, IRandomGenerator gen)
        {
            return UniformVector2SquareDistribution(rect.xMin, rect.xMax, rect.yMin, rect.yMax, gen);
        }

        public static Vector2 UniformVector2SquareDistribution(float minX, float maxX, float minY, float maxY, IRandomGenerator gen)
        {
            float x = Rand.UniformFloatDistribution(minX, maxX, gen);
            float y = Rand.UniformFloatDistribution(minY, maxY, gen);

            return new Vector2(x, y);
        }

        //-------------------

        public static Vector2 UniformVector2SquareSurfaceDistribution(IRandomGenerator gen)
        {
            return UniformVector2SquareSurfaceDistribution(0.0f, 1.0f, 0.0f, 1.0f, gen);
        }

        public static Vector2 UniformVector2SquareSurfaceDistribution(float max, IRandomGenerator gen)
        {
            return UniformVector2SquareSurfaceDistribution(0.0f, max, 0.0f, max, gen);
        }

        public static Vector2 UniformVector2SquareSurfaceDistribution(float min, float max, IRandomGenerator gen)
        {
            return UniformVector2SquareSurfaceDistribution(min, max, min, max, gen);
        }

        public static Vector2 UniformVector2SquareSurfaceDistribution(Rect rect, IRandomGenerator gen)
        {
            return UniformVector2SquareSurfaceDistribution(rect.xMin, rect.xMax, rect.yMin, rect.yMax, gen);
        }

        public static Vector2 UniformVector2SquareSurfaceDistribution(float minX, float maxX, float minY, float maxY, IRandomGenerator gen)
        {
            float sizeX = maxX - minX;
            float sizeY = maxY - minY;

            float value = Rand.UniformFloatDistribution((sizeX + sizeY) * 2, gen);
            if (value < sizeX)
                return new Vector2(minX + value, minY);
            value -= sizeX;
            if (value < sizeX)
                return new Vector2(minX + value, minY + sizeY);
            value -= sizeX;
            if (value < sizeY)
                return new Vector2(minX, minY + value);
            value -= sizeY;
            return new Vector2(minX + sizeX, minY + value);
        }
    }
}