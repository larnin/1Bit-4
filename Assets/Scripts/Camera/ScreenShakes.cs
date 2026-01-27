using UnityEngine;
using NRand;
using DG.Tweening;
using System;

[Serializable]
public abstract class ScreenShakeBase
{
    protected Vector2 m_offset = Vector2.zero;
    protected float m_rotation = 0;
    protected float m_scale = 0;
    protected bool m_init = false;

    public void Update(float deltaTime, IRandomGenerator generator)
    {
        if(!m_init)
        {
            Init();
            m_init = true;
        }

        OnUpdate(deltaTime, generator);
    }

    protected abstract void Init();
    public abstract void Start();
    protected abstract void OnUpdate(float deltaTime, IRandomGenerator generator);
    public abstract bool IsEnded();

    public Vector2 GetOffset() { return m_offset; }
    public float GetRotation() { return m_rotation; }
    public float GetScale() { return m_scale; }
}

[Serializable]
public class ScreenShake_Random : ScreenShakeBase
{
    [SerializeField] float m_dampingPow = 0;
    [SerializeField] float m_duration = 0;
    [SerializeField] Vector2 m_amplitude = Vector2.one;

    float m_time = 0;

    public ScreenShake_Random() { }

    public ScreenShake_Random(float amplitude, float duration, float dampingPow = 0)
    {
        m_amplitude = new Vector2(amplitude, amplitude);
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    public ScreenShake_Random(Vector2 amplitude, float duration, float dampingPow = 0)
    {
        m_amplitude = amplitude;
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    protected override void Init() { }

    public override void Start()
    {
        m_time = 0;
    }

    protected override void OnUpdate(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_offset = Vector2.zero;
            return;
        }

        m_offset.x = Rand.UniformFloatDistribution(-m_amplitude.x, m_amplitude.x, generator);
        m_offset.y = Rand.UniformFloatDistribution(-m_amplitude.y, m_amplitude.y, generator);

        if (m_dampingPow > 0)
        {
            float multiplier = Mathf.Pow(1 - m_time / m_duration, m_dampingPow);
            m_offset *= multiplier;
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_duration;
    }
}

[Serializable]
public class ScreenShake_RandomRotation : ScreenShakeBase
{
    [SerializeField] float m_duration = 0;
    [SerializeField] float m_dampingPow = 1;
    [SerializeField] float m_amplitude = 1;

    float m_time = 0;

    public ScreenShake_RandomRotation() { }

    public ScreenShake_RandomRotation(float amplitude, float duration, float dampingPow = 0)
    {
        m_amplitude = amplitude;
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    protected override void Init() { }

    public override void Start()
    {
        m_time = 0;
    }

    protected override void OnUpdate(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_rotation = 0;
            return;
        }

        m_rotation = Rand.UniformFloatDistribution(-m_amplitude, m_amplitude, generator);

        if (m_dampingPow > 0)
        {
            float multiplier = Mathf.Pow(1 - m_time / m_duration, m_dampingPow);
            m_rotation *= multiplier;
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_duration;
    }
}

[Serializable]
public class ScreenShake_WaveRotation : ScreenShakeBase
{
    [SerializeField] float m_amplitude = 0;
    [SerializeField] float m_frequency = 1;
    [SerializeField] float m_duration = 0;
    [SerializeField] float m_dampingPow = 0;

    float m_time = 0;
    bool m_ended = false;

    public ScreenShake_WaveRotation() { }

    public ScreenShake_WaveRotation(float amplitude, float frequency, float duration, float dampingPow = 0)
    {
        m_amplitude = amplitude;
        m_frequency = frequency;
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    protected override void Init()
    {
        
    }

    public override void Start()
    {
        m_time = 0;
        m_ended = false;
    }

    protected override void OnUpdate(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_rotation = 0;
            return;
        }

        float nextOffset = m_amplitude * Mathf.Sin(m_time * Mathf.PI * 2 * m_frequency);

        if (m_dampingPow > 0)
        {
            float t = 1 - (m_time / m_duration);
            if (t <= 0)
                nextOffset = 0;
            else
            {
                float multiplier = Mathf.Pow(t, m_dampingPow);
                nextOffset *= multiplier;
            }
        }

        if (m_time > m_duration && (Mathf.Sign(nextOffset) != Mathf.Sign(m_rotation) || nextOffset == 0))
            m_ended = true;

        m_rotation = nextOffset;

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_ended;
    }
}

[Serializable]
public class ScreenShake_RandomScale : ScreenShakeBase
{
    [SerializeField] float m_duration = 0;
    [SerializeField] float m_dampingPow = 1;
    [SerializeField] float m_amplitude = 1;

    float m_time = 0;

    public ScreenShake_RandomScale() { }

    public ScreenShake_RandomScale(float amplitude, float duration, float dampingPow = 0)
    {
        m_amplitude = amplitude;
        m_duration = duration;
        m_dampingPow = dampingPow;
    }

    protected override void Init() { }

    public override void Start()
    {
        m_time = 0;
    }

    protected override void OnUpdate(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_scale = 0;
            return;
        }

        m_scale = Rand.UniformFloatDistribution(-m_amplitude, m_amplitude, generator);

        if (m_dampingPow > 0)
        {
            float multiplier = Mathf.Pow(1 - m_time / m_duration, m_dampingPow);
            m_scale *= multiplier;
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_duration;
    }
}

[Serializable]
public class ScreenShake_ImpactScale : ScreenShakeBase
{
    [SerializeField] float m_inDuration = 0;
    [SerializeField] float m_outDuration = 1;

    [SerializeField] Ease m_inEase;
    [SerializeField] Ease m_outEase;

    [SerializeField] float m_amplitude = 0;

    float m_time = 0;

    public ScreenShake_ImpactScale() { }

    public ScreenShake_ImpactScale(float amplitude, float duration, Ease inOutEase = Ease.Linear)
    {
        m_amplitude = amplitude;
        m_inDuration = duration / 2;
        m_outDuration = duration / 2;
        m_inEase = inOutEase;
        m_outEase = inOutEase;
    }

    public ScreenShake_ImpactScale(float amplitude, float inDuration, Ease inEase, float outDuration, Ease outEase)
    {
        m_amplitude = amplitude;
        m_inDuration = inDuration;
        m_outDuration = outDuration;
        m_inEase = inEase;
        m_outEase = outEase;
    }

    protected override void Init()
    {
        
    }

    public override void Start()
    {
        m_time = 0;
    }

    protected override void OnUpdate(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_scale = 0;
            return;
        }

        if (m_time < m_inDuration)
            m_scale = DOVirtual.EasedValue(0, m_amplitude, m_time / m_inDuration, m_inEase);
        else
        {
            float t = m_time - m_inDuration;
            m_scale = DOVirtual.EasedValue(m_amplitude, 0, t / m_outDuration, m_outEase);
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_inDuration + m_outDuration;
    }
}

[Serializable]
public class ScreenShake_ImpactDirection : ScreenShakeBase
{
    [SerializeField] float m_inDuration = 0;
    [SerializeField] float m_outDuration = 1;

    [SerializeField] Ease m_inEase;
    [SerializeField] Ease m_outEase;

    [SerializeField] float m_amplitude = 0;

    [SerializeField] Vector2 m_direction;

    float m_time = 0;

    public ScreenShake_ImpactDirection() { }

    public ScreenShake_ImpactDirection(float amplitude, Vector2 direction, float duration, Ease inOutEase = Ease.Linear)
    {
        m_amplitude = amplitude;
        m_direction = direction.normalized;
        m_inDuration = duration / 2;
        m_outDuration = duration / 2;
        m_inEase = inOutEase;
        m_outEase = inOutEase;
    }

    public ScreenShake_ImpactDirection(float amplitude, Vector2 direction, float inDuration, Ease inEase, float outDuration, Ease outEase)
    {
        m_amplitude = amplitude;
        m_direction = direction.normalized;
        m_inDuration = inDuration;
        m_outDuration = outDuration;
        m_inEase = inEase;
        m_outEase = outEase;
    }

    protected override void Init()
    {
        
    }

    public override void Start()
    {
        m_time = 0;
    }

    protected override void OnUpdate(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_offset = Vector2.zero;
            return;
        }

        if (m_time < m_inDuration)
            m_offset = DOVirtual.EasedValue(0, m_amplitude, m_time / m_inDuration, m_inEase) * m_direction;
        else
        {
            float t = m_time - m_inDuration;
            m_offset = DOVirtual.EasedValue(m_amplitude, 0, t / m_outDuration, m_outEase) * m_direction;
        }

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_time > m_inDuration + m_outDuration;
    }
}

[Serializable]
public class ScreenShake_WaveDirection : ScreenShakeBase
{
    [SerializeField] float m_amplitude = 0;
    [SerializeField] float m_frequency = 1;
    [SerializeField] float m_duration = 0;
    [SerializeField] float m_dampingPow = 0;
    [SerializeField] Vector2 m_direction;

    float m_time = 0;
    bool m_ended = false;
    float m_lastOffset = 0;

    public ScreenShake_WaveDirection() { }

    public ScreenShake_WaveDirection(float amplitude, Vector2 direction, float frequency, float duration, float dampingPow = 0)
    {
        m_amplitude = amplitude;
        m_frequency = frequency;
        m_duration = duration;
        m_dampingPow = dampingPow;
        m_direction = direction.normalized;
    }

    protected override void Init()
    {
        
    }

    public override void Start()
    {
        m_time = 0;
        m_ended = false;
        m_lastOffset = 0;
    }

    protected override void OnUpdate(float deltaTime, IRandomGenerator generator)
    {
        if (IsEnded())
        {
            m_offset = Vector2.zero;
            return;
        }

        float nextOffset = m_amplitude * Mathf.Sin(m_time * Mathf.PI * 2 * m_frequency);

        if (m_dampingPow > 0)
        {
            float t = 1 - (m_time / m_duration);
            if (t <= 0)
                nextOffset = 0;
            else
            {
                float multiplier = Mathf.Pow(t, m_dampingPow);
                nextOffset *= multiplier;
            }
        }

        if (m_time > m_duration && (Mathf.Sign(nextOffset) != Mathf.Sign(m_lastOffset) || nextOffset == 0))
            m_ended = true;

        m_offset = m_direction * nextOffset;
        m_lastOffset = nextOffset;

        m_time += deltaTime;
    }

    public override bool IsEnded()
    {
        return m_ended;
    }
}