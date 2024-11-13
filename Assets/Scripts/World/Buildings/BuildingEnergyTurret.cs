using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingEnergyTurret : BuildingBase
{
    [SerializeField] float m_energyStorage = 10;
    [SerializeField] float m_energyUptake = 2;
    [SerializeField] float m_energyPerFire = 1;
    [SerializeField] float m_fireRate = 1;
    [SerializeField] float m_range = 5;
    [SerializeField] GameObject m_projectilePrefab;
    [SerializeField] GameObject m_firePrefab;

    float m_energy = 0;

    TurretBehaviour m_turret;

    GameObject m_target;

    List<Transform> m_firePoints = new List<Transform>();

    float m_fireTimer;
    int m_fireIndex;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public override void Start()
    {
        base.Start();

        m_turret = GetComponent<TurretBehaviour>(); 
        GetFirePoints(transform);
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

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Turret1;
    }

    public override float EnergyUptakeWanted()
    {
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

    protected override void Update()
    {
        base.Update();

        if (EntityList.instance == null)
            m_target = null;
        else
        {
            var target = EntityList.instance.GetNearestEntity(GetGroundCenter());
            if (target != null)
                m_target = target.gameObject;
            else m_target = null;
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
            else m_turret.SetTarget(m_target.transform.position);
        }
        
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
        if (m_energy < m_energyPerFire)
            return;

        m_energy -= m_energyPerFire;

        if (m_firePoints.Count == 0)
            return;

        if (m_fireIndex < 0 || m_fireIndex >= m_firePoints.Count)
            m_fireIndex = 0;

        Transform firePos = m_firePoints[m_fireIndex];

        if (m_firePrefab != null)
        {
            var obj = Instantiate(m_firePrefab);
            obj.transform.position = firePos.position;
            obj.transform.rotation = firePos.rotation;
        }

        if (m_projectilePrefab != null)
        {
            var obj = Instantiate(m_projectilePrefab);
            obj.transform.position = firePos.position;
            obj.transform.rotation = firePos.rotation;

            var projectile = obj.GetComponent<ProjectileBase>();
            if (projectile != null)
            {
                var target = m_target;
                if (target != null)
                {
                    projectile.SetTarget(target);
                    projectile.SetCaster(gameObject);
                    //multipliers & others stuffs
                }
            }
        }

        m_fireIndex++;
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
