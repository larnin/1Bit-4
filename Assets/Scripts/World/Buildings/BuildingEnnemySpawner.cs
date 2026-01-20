using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class BuildingEnnemySpawner : BuildingBase
{
    [SerializeField] GameObject m_mesh;
    [SerializeField] float m_appearOffset;
    [SerializeField] float m_appearDuration;
    [SerializeField] Ease m_appearCurve;

    enum State
    {
        Appear,
        Idle,
    }


    float m_timer;
    State m_state = State.Appear;

    Vector3 m_appearEndPos;
    Vector3 m_appearStartPos;
    float m_appearTimer;
    float m_wantedLight;
    CustomLight m_light;

    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();

        m_appearEndPos = m_mesh.transform.localPosition;
        m_appearStartPos = m_appearEndPos - new Vector3(0, m_appearOffset, 0);

        m_light = GetComponentInChildren<CustomLight>();
        m_wantedLight = m_light.GetRadius();

        m_mesh.transform.localScale = new Vector3(0.99f, 0.99f, 0.99f);
        UpdateAppear();

        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_subscriberList.Unsubscribe();
    }

    public override BuildingType GetBuildingType()
    {
        return BuildingType.EnnemySpawner;
    }

    public bool HaveAppeared()
    {
        return true;
    }

    protected override void OnUpdateAlways()
    {
        if (Utility.IsFrozen(gameObject) || Utility.IsDead(gameObject))
            return;

        if (EditorGridBehaviour.instance == null)
        {
            switch (m_state)
            {
                case State.Appear:
                    if (UpdateAppear())
                        m_state = State.Idle;
                    break;
                case State.Idle:
                    break;
            }
        }
    }

    bool UpdateAppear()
    {
        bool ended = false;
        m_appearTimer += Time.deltaTime;

        float normTimer = m_appearTimer / m_appearDuration;

        if (normTimer > 1)
        {
            ended = true;
            normTimer = 1;
            m_mesh.transform.localScale = Vector3.one;
        }

        var pos = DOVirtual.EasedValue(m_appearStartPos, m_appearEndPos, normTimer, m_appearCurve);
        m_mesh.transform.localPosition = pos;

        float light = DOVirtual.EasedValue(0, m_wantedLight, normTimer, m_appearCurve);
        m_light.SetRadius(light);

        return ended;
    }


    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        Event<DisplaySpawnerInfosEvent>.Broadcast(new DisplaySpawnerInfosEvent(this, e.container));
    }

    protected override void LoadImpl(JsonObject obj)
    {
        var jsonTimer = obj.GetElement("timer");
        if (jsonTimer != null && jsonTimer.IsJsonNumber())
            m_timer = jsonTimer.Float();

        var jsonState = obj.GetElement("state");
        if(jsonState != null && jsonState.IsJsonString())
        {
            if(!Enum.TryParse<State>(jsonState.String(), out m_state))
                m_state = State.Idle;
        }

        var jsonAppearStart = obj.GetElement("appearStart");
        if (jsonAppearStart != null && jsonAppearStart.IsJsonArray())
            m_appearStartPos = Json.ToVector3(jsonAppearStart.JsonArray());

        var jsonAppearEnd = obj.GetElement("appearEnd");
        if (jsonAppearEnd != null && jsonAppearEnd.IsJsonArray())
            m_appearEndPos = Json.ToVector3(jsonAppearEnd.JsonArray());

        var jsonAppearTimer = obj.GetElement("appearTimer");
        if (jsonAppearTimer != null && jsonAppearTimer.IsJsonNumber())
            m_appearTimer = jsonAppearTimer.Float();
    }

    protected override void SaveImpl(JsonObject obj)
    {
        obj.AddElement("timer", m_timer);
        obj.AddElement("state", m_state.ToString());

        obj.AddElement("appearStart", Json.FromVector3(m_appearStartPos));
        obj.AddElement("appearEnd", Json.FromVector3(m_appearEndPos));
        obj.AddElement("appearTimer", m_appearTimer);
    }
}

