using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BuildingType
{
    Tower,
    Pylon,
    BigPylon,
    SolarPanel,
    BigSolarPanel,
    PowerPlant,
    Battery,
    BigBattery,
    ResearchCenter,
    CrystalMine,
    OilPump,
    TitaniumMine,
    WaterPump,
    Turret1,
    Turret2,
    Turret3,
    Storage,
    EnnemySpawner = 100,
}

public static class BuildingTypeEx
{
    public static bool IsNode(BuildingType type)
    {
        return type == BuildingType.Tower || type == BuildingType.Pylon || type == BuildingType.BigPylon;
    }
}

public enum EnergyUptakePriority
{
    consumption,
    storage,
}

public enum BuildingPlaceType
{
    Valid,
    Unknow,
    NoResources,
    InvalidPlace,
    TooFar,
    NeedCrystal,
    NeedTitanim,
    NeedOil,
    NeedWater,
    TooCloseSolarPannel,
}

public abstract class BuildingBase : MonoBehaviour
{
    bool m_added = false;
    bool m_startCalled = false;
    bool m_asCursor = false;

    Vector3 m_localRayPoint = Vector3.zero;
    bool m_rayPointInit = false;
    Vector3 m_pos;

    float m_noHitDuration;
    float m_alarmTimer = 0;
    bool m_wasFullLife = true;

    SubscriberList m_subscriberList = new SubscriberList();

    public virtual void Awake()
    {
        m_subscriberList.Add(new Event<GetTeamEvent>.LocalSubscriber(GetTeam, gameObject));
        m_subscriberList.Add(new Event<LifeLossEvent>.LocalSubscriber(OnLifeLoss, gameObject));
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Add(new Event<ConnexionsUpdatedEvent>.Subscriber(OnConnexionUpdated));
        m_subscriberList.Subscribe();
    }

    public void SetAsCursor(bool asCursor)
    {
        m_asCursor = asCursor;
        if (m_asCursor && m_added)
            Remove();
        else if (!m_asCursor && !m_added)
            Add();
    }

    public bool IsAdded()
    {
        return m_added && !m_asCursor;
    }

    public virtual Vector3Int GetSize()
    {
        var building = Global.instance.buildingDatas.GetBuilding(GetBuildingType());
        if (building == null)
            return Vector3Int.one;

        return building.size;
    }

    public Vector3Int GetPos()
    {
        var pos = transform.position;
        return new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
    }

    public Vector3Int GetPosThread()
    {
        return new Vector3Int(Mathf.RoundToInt(m_pos.x), Mathf.RoundToInt(m_pos.y), Mathf.RoundToInt(m_pos.z));
    }

    public Vector3 GetRayPoint()
    {
        if(!m_rayPointInit)
        {
            var point = transform.Find("RayPoint");
            if(point != null)
                m_localRayPoint = point.localPosition;
            else
            {
                var center = GetGroundCenter();
                var size = GetSize();
                if (size.y < 2)
                    center.y += size.y / 2.0f;
                else center.y++;
                m_localRayPoint = center - transform.position;
            }
            m_rayPointInit = true;
        }

        return transform.position + m_localRayPoint;
    }

    public Vector3 GetGroundCenter()
    {
        return GetGroundCenter(GetBounds());
    }

    public Vector3 GetGroundCenterThread()
    {
        return GetGroundCenter(GetBoundsThread());
    }

    Vector3 GetGroundCenter(BoundsInt bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max - Vector3Int.one;

        var center = (max + min) / 2;
        center.y = min.y;

        return center;
    }

    public BoundsInt GetBounds()
    {
        return GetBounds(GetPos());
    }

    public BoundsInt GetBoundsThread()
    {
        return GetBounds(GetPosThread());
    }

    public BoundsInt GetBounds(Vector3Int pos)
    {
        var size = GetSize();
        pos.x -= (size.x - 1) / 2;
        pos.z -= (size.z - 1) / 2;

        return new BoundsInt(pos, size);
    }

    public abstract BuildingType GetBuildingType();

    public Team GetTeam()
    {
        var building = GetBuildingType();
        var b = Global.instance.buildingDatas.GetBuilding(building);
        if (b == null)
            return Team.Neutral;
        return b.team;
    }

    void GetTeam(GetTeamEvent e)
    {
        e.team = GetTeam();
    }

    void OnLifeLoss(LifeLossEvent e)
    {
        m_noHitDuration = 0;
        m_wasFullLife = false;

        if (DisplayIcons.instance == null)
            return;

        var building = GetBuildingType();
        var b = Global.instance.buildingDatas.GetBuilding(building);
        if (b == null)
            return;
        if (b.team != Team.Player)
            return;

        Vector3 pos = GetGroundCenter();

        DisplayIcons.instance.Register(pos, b.size.y, Global.instance.buildingDatas.lifeLossDisplayDuration, "BuildingDamaged", "", true, true);

        if(m_alarmTimer <= 0)
        {
            m_alarmTimer = Global.instance.buildingDatas.alarmRestartDelay;
            if (GameSystem.instance != null)
                GameSystem.instance.StartAlarm();
        }
    }

    void OnDeath(DeathEvent e)
    {
        if (GetTeam() == Team.Player)
            Event<OnBuildingDestroyedEvent>.Broadcast(new OnBuildingDestroyedEvent(GetBuildingType()));
    }

    public virtual float EnergyGeneration() { return 0; }
    public virtual float EnergyUptakeWanted() { return 0; }
    public virtual void EnergyUptake(float value) { }
    public virtual EnergyUptakePriority EnergyPriority() { return EnergyUptakePriority.consumption; }
    public virtual float EnergyStorageValue() { return 0; }
    public virtual float EnergyStorageMax() { return 0; }
    public virtual void ConsumeStoredEnergy(float value) { }
    public virtual float PlacementRadius() { return 0; }

    public virtual BuildingPlaceType CanBePlaced(Vector3Int pos) 
    {
        var grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        var bounds = GetBounds(pos);
        if (grid.grid != null)
        {
            Vector3Int min = bounds.min;
            Vector3Int max = bounds.max;

            for (int i = min.x; i < max.x; i++)
            {
                for (int k = min.z; k < max.z; k++)
                {
                    var ground = GridEx.GetBlock(grid.grid, new Vector3Int(i, min.y - 1, k));
                    if (ground != BlockType.ground)
                        return BuildingPlaceType.InvalidPlace;

                    for (int j = min.y; j < max.y; j++)
                    {
                        var block = GridEx.GetBlock(grid.grid, new Vector3Int(i, j, k));
                        if (block != BlockType.air)
                            return BuildingPlaceType.InvalidPlace;
                    }
                }
            }
        }

        //test if an other building already here
        int nbBuilding = BuildingList.instance.GetBuildingNb();
        for (int i = 0; i < nbBuilding; i++)
        {
            var b = BuildingList.instance.GetBuildingFromIndex(i);
            var otherBounds = b.GetBounds();

            if (Utility.Intersects(otherBounds, bounds))
                return BuildingPlaceType.InvalidPlace;
        }

        return BuildingPlaceType.Valid;

    }

    public virtual void OnEnable()
    {
        if (!m_startCalled)
            return;

        m_pos = transform.position;

        if (!m_asCursor)
            Add();
        else SetComponentsEnabled(false);
    }

    public virtual void Start()
    {
        m_startCalled = true;

        m_pos = transform.position;

        if (!m_added && !m_asCursor)
            Add();
        else if(m_asCursor)
            SetComponentsEnabled(false);
    }

    public virtual void OnDisable()
    {
        Remove();
    }

    public virtual void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
        Remove();
    }

    void Update()
    {
        m_pos = transform.position;

        if (!m_added && !m_asCursor)
            Add();

        if (GameInfos.instance.paused)
            return;

        OnUpdateAlways();

        if (Utility.IsFrozen(gameObject))
            return;

        if (!IsAdded())
            return;

        if (Utility.IsDead(gameObject))
            return;
        
        if(GetTeam() == Team.Player)
        {
            if (ConnexionSystem.instance != null && !ConnexionSystem.instance.IsConnected(this))
                return;

            //regen
            m_noHitDuration += Time.deltaTime;
            if(m_noHitDuration >= Global.instance.buildingDatas.regenDelay && !m_wasFullLife)
            {
                HealEvent heal = new HealEvent(Global.instance.buildingDatas.regenSpeed * Time.deltaTime, true);
                Event<HealEvent>.Broadcast(heal, gameObject);

                GetLifeEvent life = new GetLifeEvent();
                Event<GetLifeEvent>.Broadcast(life, gameObject);
                m_wasFullLife = life.lifePercent >= 1;
            }
            m_alarmTimer -= Time.deltaTime;
        }

        OnUpdate();
    }

    protected virtual void OnUpdateAlways() { }

    protected virtual void OnUpdate() { }

    void Add()
    {
        var manager = BuildingList.instance;
        if (manager != null)
        {
            m_added = true;
            manager.Register(this);
            SetComponentsEnabled(m_added);
        }
    }

    void Remove()
    {
        if (!m_added)
            return;
        var manager = BuildingList.instance;
        if (manager != null)
            manager.UnRegister(this);

        m_added = false;
        SetComponentsEnabled(m_added);
    }

    void SetComponentsEnabled(bool enabled)
    {
        var light = GetComponent<CustomLight>();
        if (light != null)
            light.enabled = enabled;
    }

    protected void DisplayGenericInfos(UIElementContainer container)
    {
        var data = Global.instance.buildingDatas.GetBuilding(GetBuildingType());
        if (data == null)
            return;

        UIElementData.Create<UIElementSimpleText>(container).SetText(data.name).SetAlignment(UIElementAlignment.center);
        UIElementData.Create<UIElementSpace>(container).SetSpace(5);
    }

    void OnConnexionUpdated(ConnexionsUpdatedEvent e)
    {
        if (ConnexionSystem.instance != null)
        {
            if (GetTeam() == Team.Player)
                SetComponentsEnabled(ConnexionSystem.instance.IsConnected(this));
        }
    }
}

