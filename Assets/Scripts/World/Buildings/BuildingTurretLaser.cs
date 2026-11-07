using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTurretLaser : BuildingTurretBase
{
    [SerializeField] float m_energyStorage = 10;
    [SerializeField] float m_energyUptake = 2;
    [SerializeField] float m_fireDuration = 5;
    [SerializeField] float m_damages = 1;
    [SerializeField] LayerMask m_hitLayer;
    [SerializeField] DamageType m_damageType = DamageType.Normal;
    [SerializeField] float m_damageTypePower = 1;
    [SerializeField] string m_fireSound;
    [SerializeField] float m_fireSoundVolume = 1;
    [SerializeField] string m_laserLayer;
    [SerializeField] Material m_laserMaterial;
    [SerializeField] float m_laserFadeTime = 0.5f;
    [SerializeField] float m_laserWidth = 0.3f;

    bool m_fireEnabled = false;
    int m_fireSoundID = -1;

    float m_energy = 0;

    SubscriberList m_subscriberList = new SubscriberList();

    LineRenderer m_laser;
    Vector3 m_laserTarget;
    float m_laserPower = 0;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Turret5;
    }

    public override void Awake()
    {
        base.Awake();

        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_subscriberList.Unsubscribe();

        if (m_fireSoundID >= 0 && SoundSystem.instance != null)
            SoundSystem.instance.StopSound(m_fireSoundID);
    }

    public override float EnergyUptakeWanted()
    {
        if (m_fireEnabled)
            return 0;

        if (m_energy < m_energyStorage)
            return m_energyUptake;

        return 0;
    }

    public override void EnergyUptake(float value)
    {
        m_energy += value * Time.deltaTime;
        if (m_energy > m_energyStorage)
            m_energy = m_energyStorage;
    }

    protected override bool CanFire()
    {
        if (m_fireEnabled && m_energy > 0)
            return true;

        var firePoint = GetCurrentFirepoint();
        if (firePoint == null)
            return false;

        if (!IsTargetVisible(firePoint.position))
            return false;

        return m_energy >= m_energyStorage;
    }

    protected override bool IsContinuousWeapon()
    {
        return true;
    }

    protected override void StartFire()
    {
        m_fireEnabled = true;

        if (SoundSystem.instance != null)
        {
            var firePoint = GetCurrentFirepoint();
            if (firePoint != null)
                m_fireSoundID = SoundSystem.instance.PlaySound(m_fireSound, firePoint.position, m_fireSoundVolume, true);
        }
    }

    protected override void EndFire()
    {
        m_fireEnabled = false;

        if (SoundSystem.instance != null)
        {
            SoundSystem.instance.StopSound(m_fireSoundID);
            m_fireSoundID = -1;
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (m_fireEnabled)
            HitTarget();

        if(m_fireEnabled)
        {
            float consumptionPercent = Time.deltaTime / m_fireDuration;
            m_energy -= m_energyStorage * consumptionPercent;
            if(m_energy <= 0)
            {
                m_energy = 0;
                EndFire();
            }
        }
    }

    protected override void OnUpdateAlways()
    {
        base.OnUpdateAlways();

        if (m_fireEnabled)
        {
            if (ConnexionSystem.instance != null && !ConnexionSystem.instance.IsConnected(this))
                EndFire();
        }

        UpdateRay();
    }

    void UpdateRay()
    {
        if (m_laser == null)
            m_laserPower = 0;

        if(m_laser == null && m_fireEnabled)
            CreateRay();

        if (m_fireEnabled)
            m_laserPower += Time.deltaTime / m_laserFadeTime;
        else m_laserPower -= Time.deltaTime / m_laserFadeTime;
        m_laserPower = Mathf.Clamp01(m_laserPower);

        if(m_laser != null)
        {
            if(m_laserPower <= 0)
            {
                Destroy(m_laser.gameObject);
                m_laser = null;
            }
            else
            {
                var firePoint = GetCurrentFirepoint();
                if (firePoint != null)
                    m_laser.SetPosition(0, firePoint.position);
                m_laser.SetPosition(1, m_laserTarget);

                var mat = m_laser.sharedMaterial;
                if(mat != null)
                {
                    float ratio = (firePoint.position - m_laserTarget).magnitude / m_laserWidth;
                    mat.SetFloat("_LengthRatio", ratio);
                    mat.SetFloat("_Intensity", m_laserPower);
                }
            }
        }
    }

    void CreateRay()
    {
        var obj = new GameObject("Ray");
        obj.transform.parent = transform;
        obj.layer = LayerMask.NameToLayer(m_laserLayer);

        var line = obj.AddComponent<LineRenderer>();
        line.startWidth = m_laserWidth;
        line.endWidth = m_laserWidth;
        line.startColor = Color.white;
        line.endColor = Color.white;

        Vector3 pos1 = transform.position;
        Vector3 pos2 = transform.position;

        line.positionCount = 2;
        Vector3[] points = new Vector3[2] { pos1, pos2 };
        line.SetPositions(points);
        line.material = new Material(m_laserMaterial);
        obj.transform.position = transform.position;

        m_laser = line;
    }

    void HitTarget()
    {
        var target = GetTarget();
        if(target == null)
        {
            EndFire();
            return;
        }

        var firePoint = GetCurrentFirepoint();
        if (firePoint == null)
            return;

        m_laserTarget = TurretBehaviour.GetTargetCenter(target);
        var dir = (m_laserTarget - firePoint.position);
        float dist = dir.magnitude;
        dir /= dist;

        float bestDistance = dist;
        var hits = Physics.RaycastAll(firePoint.position, dir, dist, m_hitLayer.value);
        var ownerTeam = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), gameObject).team;
        foreach(var oneHit in hits)
        {
            if(oneHit.distance < bestDistance)
            {
                var type = GameSystem.GetEntityType(gameObject);
                if (type == EntityType.None || type == EntityType.Quest)
                    continue;

                var currentTeam = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), oneHit.collider.gameObject).team;
                if (TeamEx.GetOppositeTeam(ownerTeam) != currentTeam)
                    continue;

                bestDistance = dist;
                target = oneHit.collider.gameObject;
                m_laserTarget = oneHit.point;
            }
        }

        var multiplier = Event<GetStatEvent>.Broadcast(new GetStatEvent(StatType.DamagesMultiplier), gameObject);
        float hitValue = Time.deltaTime * m_damages * multiplier.GetValue();

        var hit = new Hit(hitValue, gameObject, m_damageType, m_damageTypePower);
        Event<HitEvent>.Broadcast(new HitEvent(hit), target);

        //on kill recharge the turret
        if (Event<IsDeadEvent>.Broadcast(new IsDeadEvent(), target).isDead)
            EndFire();
    }

    float GetEnergy()
    {
        return m_energy;
    }

    float GetStorage()
    {
        return m_energyStorage;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Power storage").SetValueFunc(GetEnergy).SetMaxFunc(GetStorage).SetNbDigits(1).SetValueDisplayType(UIElementFillValueDisplayType.classic);
    }
}
