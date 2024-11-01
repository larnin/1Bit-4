using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRand
{
    public static class Rand
    {
        public static bool BernoulliDistribution(IRandomGenerator gen)
        {
            return BernoulliDistribution(0.5f, gen);
        }

        public static bool BernoulliDistribution(float p, IRandomGenerator gen)
        {
            return (float)(gen.Next() - gen.Min()) / (gen.Max() - gen.Min()) < p;
        }

        public static int BinomialDistribution(int rollCount, IRandomGenerator gen)
        {
            return BinomialDistribution(rollCount, 0.5f, gen);
        }

        public static int BinomialDistribution(int rollCount, float probability, IRandomGenerator gen)
        {
            int value = 0;
            for (int i = 0; i < rollCount; i++)
                if (BernoulliDistribution(probability, gen))
                    value++;
            return value;
        }

        public static int DiscreteDistribution(List<float> weights, IRandomGenerator gen)
        {
            float sum = weights.Sum();
            float currentWeight = UniformFloatDistribution(sum, gen);
            for (int i = 0; i < weights.Count; i++)
            {
                currentWeight -= weights[i];
                if (currentWeight <= 0)
                    return i;
            }
            return -1;
        }

        public static int DiscreteDistribution(Func<int, float> func, int nbItem, IRandomGenerator gen)
        {
            float sum = 0;
            for (int i = 0; i < nbItem; i++)
                sum += func(i);
            float currentWeight = UniformFloatDistribution(sum, gen);
            for (int i = 0; i < nbItem; i++)
            {
                currentWeight -= func(i);
                if (currentWeight <= 0)
                    return i;
            }
            return -1;
        }

        public static float UniformFloatDistribution(IRandomGenerator gen)
        {
            return UniformFloatDistribution(0, 1, gen);
        }
        public static float UniformFloatDistribution(float max, IRandomGenerator gen)
        {
            return UniformFloatDistribution(0, max, gen);
        }

        public static float UniformFloatDistribution(float min, float max, IRandomGenerator gen)
        {
            return ((float)gen.Next() - gen.Min()) / (gen.Max() - gen.Min()) * (max - min) + min;
        }

        public static int UniformIntDistribution(IRandomGenerator gen)
        {
            return UniformIntDistribution(0, int.MaxValue, gen);
        }

        public static int UniformIntDistribution(int max, IRandomGenerator gen)
        {
            return UniformIntDistribution(0, max, gen);
        }

        public static int UniformIntDistribution(int min, int max, IRandomGenerator gen)
        {
            return (int)(((float)gen.Next() - gen.Min()) / (gen.Max() - gen.Min()) * (max - min) + min);
        }
    }
}
