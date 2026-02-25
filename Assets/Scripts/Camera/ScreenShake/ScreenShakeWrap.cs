using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public abstract class ScreenShakeWrapBase
{
    [SerializeField] protected float m_duration = 1;

    public bool IsCompleted(ScreenShakePlayData data)
    {
        if (data.overrideDuration > 0)
            return data.time > data.overrideDuration;
        return data.time > m_duration;
    }

    public abstract float GetValue(ScreenShakePlayData data);
}

public class ScreenShakeWrapConstant : ScreenShakeWrapBase
{
    [SerializeField] float m_fadeIn = 0;
    [SerializeField] float m_fadeOut = 0;

    public override float GetValue(ScreenShakePlayData data)
    {
        float duration = data.overrideDuration > 0 ? data.overrideDuration : m_duration;

        if (data.time < 0 || data.time > duration)
            return 0;

        if(data.time < m_fadeIn)
            return data.time / m_fadeIn;
        if (data.time > duration - m_fadeOut)
            return (duration - data.time) / m_fadeOut;

        return 1;
    }
}

public class ScreenShakeWrapDamping : ScreenShakeWrapBase
{
    [SerializeField] float m_fadeIn = 0;
    [SerializeField] float m_dampingPower = 1;

    public override float GetValue(ScreenShakePlayData data)
    {
        float duration = data.overrideDuration > 0 ? data.overrideDuration : m_duration;

        if (data.time < 0 || data.time > duration)
            return 0;

        if (data.time < m_fadeIn)
            return data.time / m_fadeIn;

        duration -= m_fadeIn;
        float percent = 1 - ((data.time - m_fadeIn) / duration);

        return Mathf.Pow(percent, m_dampingPower);
    }
}

public class ScreenShakeWrapCurve : ScreenShakeWrapBase
{
    [SerializeField] AnimationCurve m_curve;

    public override float GetValue(ScreenShakePlayData data)
    {
        return m_curve.Evaluate(data.time);
    }
}