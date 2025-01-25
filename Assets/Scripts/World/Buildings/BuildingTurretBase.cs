using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BuildingTurretBase : BuildingBase
{
    [HideIf("@(this.IsContinuousWeapon())")]
    [SerializeField] float m_fireRate = 1;
    [SerializeField] float m_range = 5;
    [HideIf("@(this.IsContinuousWeapon())")]
    [SerializeField] float m_recoilDistance = 0.5f;
    [HideIf("@(this.IsContinuousWeapon())")]
    [SerializeField] float m_recoilDuration = 0.5f;
    [HideIf("@(this.IsContinuousWeapon())")]
    [SerializeField] Transform m_recoilTarget;
    [SerializeField] ParticleSystem m_fireParticles;

    TurretBehaviour m_turret;

    GameObject m_target;

    List<Transform> m_firePoints = new List<Transform>();

    float m_fireTimer;
    int m_fireIndex;
    bool m_firing;

    float m_recoilTimer;
    Vector3 m_recoilInitialPosition;

    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();

        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Subscribe();
    }

    public override void Start()
    {
        base.Start();

        m_turret = GetComponent<TurretBehaviour>(); 
        GetFirePoints(transform);

        m_recoilTimer = -1;

        if (m_recoilTarget != null)
            m_recoilInitialPosition = m_recoilTarget.localPosition;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_subscriberList.Unsubscribe();
    }

    void GetFirePoints(Transform transform)
    {
        int nbChild = transform.childCount;
        for (int i = 0; i < nbChild; i++)
        {
            var child = transform.GetChild(i);
            if (child.name == "FirePoint")
                m_firePoints.Add(child);

            GetFirePoints(child);
        }
    }

    protected override void OnUpdate()
    {
        ProcessRecoil();

        if (EntityList.instance == null || BuildingList.instance == null)
            m_target = null;
        else
        {
            Team targetTeam = TeamEx.GetOppositeTeam(GetTeam());
            m_target = null;
            var entity = EntityList.instance.GetNearestEntity(GetGroundCenter(), targetTeam, AliveType.Alive);
            if (entity != null)
                m_target = entity.gameObject;
            if (m_target == null)
            {
                var building = BuildingList.instance.GetNearestBuilding(GetGroundCenter(), targetTeam, AliveType.Alive);
                if (building != null)
                    m_target = building.gameObject;
            }
        }

        if(m_target != null)
        {
            float dist = (GetGroundCenter() - m_target.transform.position).sqrMagnitude;
            if (dist > m_range * m_range)
                m_target = null;
        }

        if(m_turret != null)
        {
            if (m_target == null)
                m_turret.SetNoTarget();
            else m_turret.SetTarget(TurretBehaviour.GetTargetCenter(m_target));
        }

        if (IsContinuousWeapon())
            UpdateContinuousTurret();
        else UpdateBulletTurret();
    }

    void UpdateBulletTurret()
    {
        float rateTimer = 1 / m_fireRate;
        m_fireTimer += Time.deltaTime;
        if (m_target != null && (m_turret == null || m_turret.CanFire()))
        {
            while (m_fireTimer >= rateTimer)
            {
                m_fireTimer -= rateTimer;
                TryFire();
            }
        }
        else if (m_fireTimer > rateTimer)
            m_fireTimer = rateTimer;
    }

    void TryFire()
    {
        if (m_fireIndex < 0 || m_fireIndex >= m_firePoints.Count)
            m_fireIndex = 0;

        if (!CanFire())
            return;

        var firePos = GetCurrentFirepoint();

        if(m_fireParticles != null)
        {
            m_fireParticles.Play();
        }

        Fire();
        StartRecoil();

        m_fireIndex++;
    }

    void UpdateContinuousTurret()
    {
        bool nextFiring = true;

        if (m_target == null || m_turret == null || !m_turret.CanFire())
            nextFiring = false;

        if (nextFiring && !CanFire())
            nextFiring = false;


        if(nextFiring != m_firing)
        {
            m_firing = nextFiring;

            if (m_firing)
            {
                StartFire();
                if (m_fireParticles != null)
                    m_fireParticles.Play();
            }
            else
            {
                EndFire();
                if (m_fireParticles != null)
                    m_fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    void StartRecoil()
    {
        m_recoilTimer = 0;
    }

    void ProcessRecoil()
    {
        if (m_recoilTimer < 0)
            return;

        m_recoilTimer += Time.deltaTime;

        float normDuration = 1 - (m_recoilTimer / m_recoilDuration);
        if (normDuration < 0)
        {
            normDuration = 0;
            m_recoilTimer = -1;
        }

        float dist = -normDuration * m_recoilDistance;
        Vector3 pos = m_recoilInitialPosition + new Vector3(0, 0, dist);

        if (m_recoilTarget != null)
            m_recoilTarget.localPosition = pos;
    }

    protected Transform GetCurrentFirepoint()
    {
        if (m_fireIndex >= m_firePoints.Count)
            return null;

        return m_firePoints[m_fireIndex];
    }

    protected GameObject GetTarget()
    {
        return m_target;
    }

    void OnDeath(DeathEvent e)
    {
        if (IsContinuousWeapon() && m_firing)
        {
            EndFire();
            m_firing = false;
        }
    }

    protected abstract bool IsContinuousWeapon();
    protected abstract bool CanFire();
    protected virtual void StartFire() { }
    protected virtual void EndFire() { }
    protected virtual void Fire() { }
}
