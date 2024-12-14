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
    EnnemySpawner = 100,
}

public enum EnergyUptakePriority
{
    consumption,
    storage,
}

public abstract class BuildingBase : MonoBehaviour
{
    bool m_added = false;
    bool m_startCalled = false;
    bool m_asCursor = false;

    Vector3 m_localRayPoint = Vector3.zero;
    bool m_rayPointInit = false;
    Vector3 m_pos;

    SubscriberList m_subscriberList = new SubscriberList();

    public virtual void Awake()
    {
        m_subscriberList.Add(new Event<GetTeamEvent>.LocalSubscriber(GetTeam, gameObject));
        m_subscriberList.Add(new Event<LifeLossEvent>.LocalSubscriber(OnLifeLoss, gameObject));
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

    public Vector3Int GetSize()
    {
        var building = Global.instance.buildingDatas.GetBuilding(GetBuildingType());
        if (building == null)
            return Vector3Int.one;

        return building.size;
    }

    public Vector3Int GetPos()
    {
        return new Vector3Int(Mathf.RoundToInt(m_pos.x), Mathf.RoundToInt(m_pos.y), Mathf.RoundToInt(m_pos.z));
    }

    public Vector3 GetRayPoint()
    {
        if(!m_rayPointInit)
        {
            var point = transform.Find("RayPoint");
            if(point != null)
            {
                m_localRayPoint = point.localPosition;
                m_rayPointInit = true;
            }
        }

        return transform.position + m_localRayPoint;
    }

    public Vector3 GetGroundCenter()
    {
        Vector3 pos = GetPos();
        Vector3 size = GetSize();
        size -= Vector3.one;

        return pos + size / 2;
    }

    public BoundsInt GetBounds()
    {
        return GetBounds(GetPos());
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
    }

    public virtual float EnergyGeneration() { return 0; }
    public virtual float EnergyUptakeWanted() { return 0; }
    public virtual void EnergyUptake(float value) { }
    public virtual EnergyUptakePriority EnergyPriority() { return EnergyUptakePriority.consumption; }
    public virtual float EnergyStorageValue() { return 0; }
    public virtual float EnergyStorageMax() { return 0; }
    public virtual void ConsumeStoredEnergy(float value) { }
    public virtual float PlacementRadius() { return 0; }

    public virtual bool CanBePlaced(Vector3Int pos) 
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
                        return false;

                    for (int j = min.y; j < max.y; j++)
                    {
                        var block = GridEx.GetBlock(grid.grid, new Vector3Int(i, j, k));
                        if (block != BlockType.air)
                            return false;
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
                return false;
        }

        return true;

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

    protected virtual void Update()
    {
        m_pos = transform.position;

        if (!m_added && !m_asCursor)
            Add();
    }

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
}

