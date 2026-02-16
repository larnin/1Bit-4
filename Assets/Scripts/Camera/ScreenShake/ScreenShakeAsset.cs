using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ScreenShakeInspector
{
    public ScreenShakeAsset asset;
    public float intensity = 1;
    public float power = 1;
}

public class ScreenShakePlayData
{
    public float intensity = 1;
    public float time = 0;
    public float overrideDuration = -1;
    public int seed = 0;
    public float power = 1;
}

[CreateAssetMenu(fileName = "ScreenShake", menuName = "Game/ScreenShake", order = 1)]
public class ScreenShakeAsset : SerializedScriptableObject
{
    [SerializeField] ScreenShakeOscilatorBase m_offsetOscilator;
    [SerializeField] ScreenShakeWrapBase m_offsetWrap;
    [SerializeField] bool m_freezeOffsetX = false;
    [SerializeField] bool m_freezeOffsetY = false;

    [SerializeField] ScreenShakeOscilatorBase m_rotationOscilator;
    [SerializeField] ScreenShakeWrapBase m_rotationWrap;

    [SerializeField] ScreenShakeOscilatorBase m_orthographicSizeOscilator;
    [SerializeField] ScreenShakeWrapBase m_orthographicSizeWrap;

    public bool IsCompleted(ScreenShakePlayData data)
    {
        bool completed = true;

        if (m_offsetWrap != null && !m_offsetWrap.IsCompleted(data))
            completed = false;
        if (m_rotationWrap != null && !m_rotationWrap.IsCompleted(data))
            completed = false;
        if (m_orthographicSizeWrap != null && !m_orthographicSizeWrap.IsCompleted(data))
            completed = false;

        return completed;
    }

    public Vector2 GetOffset(ScreenShakePlayData data)
    {
        if(m_offsetOscilator == null)
            return Vector2.zero;

        Vector2 offset = m_offsetOscilator.GetVector(data);
        if (m_offsetWrap != null)
            offset *= m_offsetWrap.GetValue(data);

        if (m_freezeOffsetX)
            offset.x = 0;
        if (m_freezeOffsetY)
            offset.y = 0;

        //isometric camera
        Vector2 realOffset = new Vector2(offset.x + offset.y, offset.x - offset.y) / 2;

        return realOffset;
    }

    public float GetRotation(ScreenShakePlayData data)
    {
        if(m_rotationOscilator == null)
            return 0;

        float rot = m_rotationOscilator.GetFloat(data);
        if (m_rotationWrap != null)
            rot *= m_rotationWrap.GetValue(data);

        return rot;
    }

    public float GetOrthographicSizeOffset(ScreenShakePlayData data)
    {
        if (m_orthographicSizeOscilator == null)
            return 0;

        float size = m_orthographicSizeOscilator.GetFloat(data);
        if (m_orthographicSizeWrap != null)
            size *= m_orthographicSizeWrap.GetValue(data);

        return size;
    }
}

