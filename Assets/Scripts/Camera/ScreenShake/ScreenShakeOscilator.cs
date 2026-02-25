using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;
using Noise;

[Serializable]
public abstract class ScreenShakeOscilatorBase
{
    public abstract float GetFloat(ScreenShakePlayData data);

    public abstract Vector2 GetVector(ScreenShakePlayData data);
}

[Serializable]
public class ScreenShakeOscilatorRandom : ScreenShakeOscilatorBase
{
    [SerializeField] float m_amplitude = 1;

    public override float GetFloat(ScreenShakePlayData data)
    {
        var rand = StaticRandomGenerator<MT19937>.Get();
        return Rand.UniformFloatDistribution(-m_amplitude, m_amplitude, rand) * data.intensity;
    }

    public override Vector2 GetVector(ScreenShakePlayData data)
    {
        var rand = StaticRandomGenerator<MT19937>.Get();
        return Rand2D.UniformVector2CircleDistribution(m_amplitude, rand) * data.intensity;
    }
}

[Serializable]
class ScreenShakeOscilatorPerlin : ScreenShakeOscilatorBase
{
    [SerializeField] float m_amplitude = 1;
    [SerializeField] int m_frequency = 1;

    public override float GetFloat(ScreenShakePlayData data)
    {
        return Perlin.GetStatic(m_amplitude, m_frequency, data.seed, data.time);
    }

    public override Vector2 GetVector(ScreenShakePlayData data)
    {
        Vector2 offset = Vector2.zero;
        for(int i = 0; i < 2; i++)
            offset[i] = Perlin.GetStatic(m_amplitude, m_frequency, data.seed + i, data.time);

        return offset;
    }
}

[Serializable]
class ScreenShakeOscilatorCurve : ScreenShakeOscilatorBase
{
    [SerializeField] AnimationCurve m_curveX;
    [SerializeField] AnimationCurve m_curveY;

    public override float GetFloat(ScreenShakePlayData data)
    {
        if (m_curveX == null)
            return 0;

        return m_curveX.Evaluate(data.time);
    }

    public override Vector2 GetVector(ScreenShakePlayData data)
    {
        Vector2 offset = Vector2.zero;

        if (m_curveX != null)
            offset.x = m_curveX.Evaluate(data.time);
        if (m_curveY != null)
            offset.y = m_curveY.Evaluate(data.time);

        return offset;
    }
}