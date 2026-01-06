using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class BuildingMonolith : BuildingBase
{
    enum State
    {
        Idle,
        AngryStart,
        AngryLoop,
        Wave,
    }

    const int MaxOrbNum = 3;

    [SerializeField] Transform m_orbsPivot;
    [SerializeField] float m_orbCenterHeight = 5;
    [SerializeField] float m_orbIdleShakeAmplitude = 0.5f;
    [SerializeField] float m_orbIdleShakeFrequency = 2.0f;
    [SerializeField] float m_orbIdleSize = 0.2f;
    [SerializeField] float m_orbIdleRotationSpeed = 1;
    [SerializeField] float m_orbAngrySize = 0.5f;
    [SerializeField] float m_orbAngryShakeAmplitude = 0.5f;
    [SerializeField] float m_orbAngryTransition = 0.5f;
    [SerializeField] float m_orbWaveDistance = 2;
    [SerializeField] float m_orbWaveStartDuration = 0.2f;
    [SerializeField] Ease m_orbWaveStartCurve = Ease.Linear;
    [SerializeField] float m_orbWaveWaitDuration = 0.2f;
    [SerializeField] float m_orbWaveEndDuration = 1.5f;
    [SerializeField] Ease m_orbWaveEndCurve = Ease.Linear;
    [SerializeField] GameObject m_orbWavePrefab;

    State m_state = State.Idle;
    float m_timer = 0;

    float m_rotationTimer = 0;

    List<Transform> m_orbParts = new List<Transform>();

    SubscriberList m_subscriberList = new SubscriberList();

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Monolith;
    }

    public override void Awake()
    {
        base.Awake();
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();

        if(m_orbsPivot != null)
        {
            for(int i = 0; i < m_orbsPivot.childCount; i++)
            {
                if (i < MaxOrbNum)
                    m_orbParts.Add(m_orbsPivot.GetChild(i));
                else m_orbsPivot.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        m_subscriberList.Unsubscribe();
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);
    }

    protected override void OnUpdateAlways()
    {
        if (Utility.IsFrozen(gameObject) || Utility.IsDead(gameObject))
            return;

        m_rotationTimer += Time.deltaTime;

        switch (m_state)
        {
            case State.Idle:
                UpdateIdleOrbMovement();
                if (m_rotationTimer > 10)
                    StartAngry();
                break;
            case State.AngryStart:
                UpdateAngryStart();
                break;
            case State.AngryLoop:
                UpdateAngryLoop();

                m_timer += Time.deltaTime;
                if (m_timer > 5)
                    StartWave();
                break;
            case State.Wave:
                UpdateWave();
                break;
        }

    }

    void UpdateIdleOrbMovement()
    {
        UpdateOrbShape(1, m_orbIdleSize);

        float height = m_orbCenterHeight + Mathf.Sin(m_rotationTimer * m_orbIdleShakeFrequency * 2 * Mathf.PI) * m_orbIdleShakeAmplitude;
        m_orbsPivot.localPosition = new Vector3(0, height, 0);
    }

    void UpdateAngryStart()
    {
        m_timer += Time.deltaTime;
        if (m_timer >= m_orbAngryTransition)
        {
            m_state = State.AngryLoop;
            m_timer = 0;
        }

        float normTimer = m_timer / m_orbAngryTransition;

        UpdateOrbShape(1, m_orbAngrySize * normTimer + m_orbIdleSize * (1 - normTimer));

        float height = m_orbCenterHeight + Mathf.Sin(m_rotationTimer * m_orbIdleShakeFrequency * 2 * Mathf.PI) * m_orbIdleShakeAmplitude * (1 - normTimer);
        var shake = Rand2D.UniformVector2CircleDistribution(m_orbAngryShakeAmplitude, StaticRandomGenerator<MT19937>.Get()) * normTimer;
        m_orbsPivot.localPosition = new Vector3(shake.x, height, shake.y);
    }

    void UpdateAngryLoop()
    {
        UpdateOrbShape(1, m_orbAngrySize);

        var shake = Rand2D.UniformVector2CircleDistribution(m_orbAngryShakeAmplitude, StaticRandomGenerator<MT19937>.Get());
        m_orbsPivot.localPosition = new Vector3(shake.x, m_orbCenterHeight, shake.y);
    }

    void UpdateWave()
    {
        float percent = 1;
        float shakePower = 0;
        m_timer += Time.deltaTime;
        if (m_timer < m_orbWaveStartDuration)
        {
            percent = m_timer / m_orbWaveStartDuration;
            percent = DOVirtual.EasedValue(1, 0, percent, m_orbWaveStartCurve);
        }
        else if (m_timer < m_orbWaveStartDuration + m_orbWaveWaitDuration)
            percent = 0;
        else if (m_timer < m_orbWaveStartDuration + m_orbWaveWaitDuration + m_orbWaveEndDuration)
        {
            percent = 1 - ((m_timer - m_orbWaveStartDuration - m_orbWaveWaitDuration) / m_orbWaveEndDuration);
            percent = DOVirtual.EasedValue(1, 0, percent, m_orbWaveEndCurve);
            if (percent > 0.5f)
                shakePower = (percent - 0.5f) / 0.5f;
        }
        else
        {
            m_state = State.AngryLoop;
            m_timer = 0;
        }

        UpdateOrbShape(percent, m_orbAngrySize);

        var shake = Rand2D.UniformVector2CircleDistribution(m_orbAngryShakeAmplitude * shakePower, StaticRandomGenerator<MT19937>.Get());
        m_orbsPivot.localPosition = new Vector3(shake.x, m_orbCenterHeight, shake.y);
    }

    void UpdateOrbShape(float lerp, float scale)
    {
        Vector3 pivotAngle = Vector3.zero;
        pivotAngle.x = 100 * Mathf.Cos(m_rotationTimer * m_orbIdleRotationSpeed);
        pivotAngle.y = 60 * Mathf.Sin(m_rotationTimer * 1.27f * m_orbIdleRotationSpeed);
        pivotAngle.z = 40 * Mathf.Cos(m_rotationTimer * 1.89f * m_orbIdleRotationSpeed);
        pivotAngle *= lerp;
        m_orbsPivot.localRotation = Quaternion.Euler(pivotAngle);

        for (int i = 0; i < m_orbParts.Count; i++)
        {
            float angle = i * Mathf.PI * 2 / 3 + m_rotationTimer / 25;
            m_orbParts[i].localPosition = m_orbWaveDistance * (1 - lerp) * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 euler = Vector3.zero;
            euler[i] = 45 * lerp;
            euler.y -= Mathf.Rad2Deg * angle * (1 - lerp);
            m_orbParts[i].localRotation = Quaternion.Euler(euler);
            m_orbParts[i].localScale = Vector3.one * scale;
        }
    }

    public void StartAngry()
    {
        m_state = State.AngryStart;
        m_timer = 0;
    }

    public bool StartWave()
    {
        if (m_state != State.AngryLoop)
            return false;

        m_state = State.Wave;
        m_timer = 0;

        if(m_orbWavePrefab != null)
        {
            var obj = Instantiate(m_orbWavePrefab);
            obj.transform.parent = m_orbsPivot;
            obj.transform.localPosition = Vector3.zero;
        }

        return true;
    }

    protected override void SaveImpl(JsonObject obj)
    {
        //todo
    }

    protected override void LoadImpl(JsonObject obj)
    {
        //todo
    }
}
