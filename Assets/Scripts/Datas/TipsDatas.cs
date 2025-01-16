using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRand;
using UnityEngine;

[Serializable]
public class OneTipData
{
    [Multiline]
    public string tip;
    public float weight = 1;
}

[Serializable]
public class TipsDatas
{
    public List<OneTipData> tips;

    public int GetRandomTipIndex(int lastIndex)
    {
        if (tips.Count == 0)
            return -1;

        if (tips.Count == 1)
            return 0;

        List<float> weights = new List<float>();
        for(int i = 0; i < tips.Count; i++)
        {
            if (i == lastIndex)
                continue;

            weights.Add(tips[i].weight);
        }

        int newIndex = Rand.DiscreteDistribution(weights, StaticRandomGenerator<MT19937>.Get());
        if (lastIndex >= 0 && newIndex >= lastIndex)
            newIndex++;

        return newIndex;
    }
}
