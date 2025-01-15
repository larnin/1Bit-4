using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTurretFlameThrower : BuildingTurretBase
{
    [SerializeField] ResourceType m_resourceConsumption = ResourceType.Oil;
    [SerializeField] float m_consumption = 1;
    [SerializeField] float m_damages = 1;
    [SerializeField] LayerMask m_hitLayer;
    [SerializeField] DamageType m_damageType = DamageType.Fire;
    [SerializeField] float m_damageTypePower = 1;
    [SerializeField] float m_coneAngle = 20;
    [SerializeField] float m_coneRange = 10;
    [SerializeField] float m_coneProgressionSpeed = 5;
    [SerializeField] float m_coneSphereCount = 4;
    [SerializeField] bool m_debugDisplaySpheres;
    [SerializeField] ParticleSystem m_particles;

    bool m_fireEnabled = false;

    SubscriberList m_subscriberList = new SubscriberList();

    class BubbleInfos
    {
        //static values
        public float radius;
        public float distance;
        public float delay;

        //variable values
        public Vector3 pos;
        public bool hit;
    }

    List<BubbleInfos> m_bubbles = new List<BubbleInfos>();

    class HistoryInfo
    {
        public float time;
        public Quaternion rot;
        public Vector3 origin;
        public bool fireEnabled;
    }

    List<HistoryInfo> m_historyInfos = new List<HistoryInfo>();

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Turret2;
    }

    public override void Awake()
    {
        base.Awake();

        CreateBubbles();

        if (m_particles != null)
            m_particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_subscriberList.Unsubscribe();
    }

    protected override bool CanFire()
    {
        if (ResourceSystem.instance == null)
            return false;

        if (!ResourceSystem.instance.HaveResource(m_resourceConsumption))
            return false;

        float resourcesFrame = m_consumption * Time.deltaTime;
        return ResourceSystem.instance.GetResourceStored(m_resourceConsumption) >= resourcesFrame;
    }

    protected override bool IsContinuousWeapon()
    {
        return true;
    }

    protected override void StartFire()
    {
        m_fireEnabled = true;

        if (m_particles != null)
            m_particles.Play(true);
    }

    protected override void EndFire()
    {
        m_fireEnabled = false;

        if (m_particles != null)
            m_particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if(m_fireEnabled)
        {
            if (ResourceSystem.instance == null)
                return;

            if (!ResourceSystem.instance.HaveResource(m_resourceConsumption))
                return;

            float resourcesFrame = m_consumption * Time.deltaTime;
            ResourceSystem.instance.RemoveResource(m_resourceConsumption, resourcesFrame);
        }
    }

    protected override void OnUpdateAlways()
    {
        base.OnUpdateAlways();

        var firePoint = GetCurrentFirepoint();
        if(firePoint != null)
        {
            var point = new HistoryInfo();
            point.time = Time.time;
            point.origin = firePoint.position;
            point.rot = firePoint.rotation;
            point.fireEnabled = m_fireEnabled;
            m_historyInfos.Add(point);
        }

        float maxTime = Time.time - (m_coneRange / m_coneProgressionSpeed);

        while (m_historyInfos.Count > 0 && m_historyInfos[0].time < maxTime)
            m_historyInfos.RemoveAt(0);

        UpdateBubbles();
        TestHits();

        if (m_debugDisplaySpheres)
            DebugDisplayBubbles();
    }

    void CreateBubbles()
    {
        m_bubbles.Clear();

        float testDistance = Mathf.Sqrt(m_coneRange);
        float unitDist = testDistance / m_coneSphereCount;
        float tanA = Mathf.Tan(Mathf.Deg2Rad * m_coneAngle / 2);


        for (int i = 0; i < m_coneSphereCount; i++)
        {
            float dist = unitDist * (i + 0.5f);
            dist = dist * dist;

            var b = new BubbleInfos();
            b.distance = dist;
            b.radius = tanA * b.distance;
            b.delay = dist / m_coneProgressionSpeed;

            m_bubbles.Add(b);
        }
    }

    void UpdateBubbles()
    {
        foreach(var b in m_bubbles)
        {
            float t = Time.time - b.delay;

            var i = GetInfosAt(t);
            if (i == null)
                continue;

            var forward = i.rot * new Vector3(0, 0, 1);
            b.pos = forward * b.distance + i.origin;
            b.hit = i.fireEnabled;
        }
    }

    void TestHits()
    {
        List<GameObject> targets = new List<GameObject>();

        GetTeamEvent currentTeam = new GetTeamEvent();
        Event<GetTeamEvent>.Broadcast(currentTeam, gameObject);

        if (currentTeam.team == Team.Neutral)
            return;

        var opposite = TeamEx.GetOppositeTeam(currentTeam.team);

        foreach (var b in m_bubbles)
        {
            if (!b.hit)
                return;

            var hits = Physics.OverlapSphere(b.pos, b.radius, m_hitLayer.value);

            foreach (var col in hits)
            {
                if (targets.Contains(col.gameObject))
                    continue;

                GetTeamEvent team = new GetTeamEvent();
                Event<GetTeamEvent>.Broadcast(team, col.gameObject);
                if (team.team != opposite)
                    continue;

                GetLifeEvent life = new GetLifeEvent();
                Event<GetLifeEvent>.Broadcast(life, col.gameObject);
                if (life.life <= 0)
                    continue;

                targets.Add(col.gameObject);
            }
        }

        var multiplier = new GetStatEvent(StatType.DamagesMultiplier);
        Event<GetStatEvent>.Broadcast(multiplier, gameObject);

        var hit = new Hit(m_damages * Time.deltaTime * multiplier.GetValue(), gameObject, m_damageType, m_damageTypePower);
        foreach (var t in targets)
            Event<HitEvent>.Broadcast(new HitEvent(hit), t);
    }

    HistoryInfo GetInfosAt(float time)
    {
        HistoryInfo best = null;

        foreach(var h in m_historyInfos)
        {
            if (best == null || Mathf.Abs(time - h.time) < Mathf.Abs(time - best.time))
                best = h;
        }

        return best;
    }

    void DebugDisplayBubbles()
    {
        foreach (var b in m_bubbles)
        {
            Color c = b.hit ? Color.blue : Color.red;
            DebugDraw.Sphere(b.pos, b.radius, c);
        }
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        
    }
}

