using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Noise
{
    public class Perlin
    {
        int m_size;
        int m_frequency;
        float m_amplitude;

        RandomHash m_generator;

        public Perlin(int size, float amplitude, int frequency, Int32 seed)
        {
            m_size = size;
            m_frequency = frequency;
            m_amplitude = amplitude;

            m_generator = new RandomHash(seed);
        }

        public float Get(float x, Lerp.Operator o = Lerp.Operator.Square)
        {
            float dec;
            int x1, x2;
            SplitValue(x, m_size, m_frequency, out x1, out x2, out dec);

            float v1 = Rand.UniformFloatDistribution(-m_amplitude, m_amplitude, m_generator.Set(x1));
            float v2 = Rand.UniformFloatDistribution(-m_amplitude, m_amplitude, m_generator.Set(x2));

            return Lerp.LerpValue(v1, v2, dec, o);
        }

        static float Dot2D(int x, int y, float decX, float decY, RandomHash generator)
        {
            var value = Rand2D.UniformVector2SquareSurfaceDistribution(-1, 1, generator.Set(x, y));

            return value.x * decX + value.y * decY;
        }

        public float Get(float x, float y, Lerp.Operator o = Lerp.Operator.Square)
        {
            float decX, decY;
            int x1, x2, y1, y2;

            SplitValue(x, m_size, m_frequency, out x1, out x2, out decX);
            SplitValue(y, m_size, m_frequency, out y1, out y2, out decY);

            float v1 = Dot2D(x1, y1, decX, decY, m_generator);
            float v2 = Dot2D(x2, y1, decX - 1, decY, m_generator);
            float v3 = Dot2D(x1, y2, decX, decY - 1, m_generator);
            float v4 = Dot2D(x2, y2, decX - 1, decY - 1, m_generator);

            return Lerp.LerpValue2D(v1, v2, v3, v4, decX, decY, o) * m_amplitude;
        }

        static float Dot3D(int x, int y, int z, float decX, float decY, float decZ, RandomHash generator)
        {
            var value = Rand3D.UniformVector3BoxSurfaceDistribution(-1, 1, generator.Set(x, y, z));
            return value.x * decX + value.y * decY + value.z * decZ;
        }


        public float Get(float x, float y, float z, Lerp.Operator o = Lerp.Operator.Square)
        {
            float decX, decY, decZ;
            int x1, x2, y1, y2, z1, z2;

            SplitValue(x, m_size, m_frequency, out x1, out x2, out decX);
            SplitValue(y, m_size, m_frequency, out y1, out y2, out decY);
            SplitValue(z, m_size, m_frequency, out z1, out z2, out decZ);

            float v1 = Dot3D(x1, y1, z1, decX, decY, decZ, m_generator);
            float v2 = Dot3D(x2, y1, z1, decX - 1, decY, decZ, m_generator);
            float v3 = Dot3D(x1, y2, z1, decX, decY - 1, decZ, m_generator);
            float v4 = Dot3D(x2, y2, z1, decX - 1, decY - 1, decZ, m_generator);
            float v5 = Dot3D(x1, y1, z2, decX, decY, decZ - 1, m_generator);
            float v6 = Dot3D(x2, y1, z2, decX - 1, decY, decZ - 1, m_generator);
            float v7 = Dot3D(x1, y2, z2, decX, decY - 1, decZ - 1, m_generator);
            float v8 = Dot3D(x2, y2, z2, decX - 1, decY - 1, decZ - 1, m_generator);

            return Lerp.LerpValue3D(v1, v2, v3, v4, v5, v6, v7, v8, decX, decY, decZ, o);
        }

        static void SplitValue(float value, int size, int frequency, out int outX1, out int outX2, out float outDec)
        {
            if (value < 0)
                value = (value % size + size) % size;
            else value = value % size;
            //value in [0;size] bouds

            float x = value / size * frequency;

            outDec = x - Mathf.Floor(x);

            outX1 = Mathf.FloorToInt(x);
            outX2 = outX1 + 1;
            if (outX2 >= frequency)
                outX2 = 0;
        }

        public static float GetStatic(float amplitude, int frequency, Int32 seed, float x, Lerp.Operator o = Lerp.Operator.Square)
        {
            RandomHash generator = new RandomHash(seed);

            float dec;
            int x1, x2;
            SplitValue(x, 1, frequency, out x1, out x2, out dec);

            float v1 = Rand.UniformFloatDistribution(-amplitude, amplitude, generator.Set(x1));
            float v2 = Rand.UniformFloatDistribution(-amplitude, amplitude, generator.Set(x2));

            return Lerp.LerpValue(v1, v2, dec, o);
        }
    }
}