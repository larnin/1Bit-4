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
    PositionLocked,
}

public abstract class BuildingBase : MonoBehaviour
{
    [SerializeField] Transform m_meshComponent;

    bool m_added = false;
    bool m_startCalled = false;
    bool m_asCursor = false;

    Vector3 m_localRayPoint = Vector3.zero;
    bool m_rayPointInit = false;
    Vector3 m_pos;

    float m_noHitDuration;
    float m_alarmTimer = 0;
    bool m_wasFullLife = true;
    Team m_team;

    Rotation m_rotation = Rotation.rot_0;

    SubscriberList m_subscriberList = new SubscriberList();

    public virtual void Awake()
    {
        m_subscriberList.Add(new Event<GetTeamEvent>.LocalSubscriber(GetTeam, gameObject));
        m_subscriberList.Add(new Event<LifeLossEvent>.LocalSubscriber(OnLifeLoss, gameObject));
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Add(new Event<ConnexionsUpdatedEvent>.Subscriber(OnConnexionUpdated));
        m_subscriberList.Subscribe();

        m_team = GetDefaultTeam();
    }

    public void SetAsCursor(bool asCursor)
    {
        m_asCursor = asCursor;
        if (m_asCursor && m_added)
            Remove();
        else if (!m_asCursor && !m_added)
            Add();

        if (asCursor)
            SetRotation(RotationEx.RandomRotation());
    }

    public bool IsAdded()
    {
        return m_added && !m_asCursor && EditorGridBehaviour.instance == null;
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

    public Team GetDefaultTeam()
    {
        var building = GetBuildingType();
        var b = Global.instance.buildingDatas.GetBuilding(building);
        if (b == null)
            return Team.Neutral;
        return b.team;
    }

    public Team GetTeam()
    {
        return m_team;
    }

    void GetTeam(GetTeamEvent e)
    {
        e.team = GetTeam();
    }

    public void SetTeam(Team team)
    {
        m_team = team;
    }

    void OnLifeLoss(LifeLossEvent e)
    {
        m_noHitDuration = 0;
        m_wasFullLife = false;

        if (DisplayIconsV2.instance == null)
            return;

        var building = GetBuildingType();
        var b = Global.instance.buildingDatas.GetBuilding(building);
        if (b == null)
            return;
        if (b.team != Team.Player)
            return;

        Vector3 pos = GetGroundCenter();

        DisplayIconsV2.instance.Register(pos, b.size.y, Global.instance.buildingDatas.lifeLossDisplayDuration, "BuildingDamaged", "", true, true);

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
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        var bounds = GetBounds(pos);
        if (grid.grid != null)
        {
            Vector3Int min = bounds.min;
            Vector3Int max = bounds.max;

            for (int i = min.x; i < max.x; i++)
            {
                for (int k = min.z; k < max.z; k++)
                {
                    var ground = GridEx.GetBlock(grid.grid, GridEx.GetRealPosFromLoop(grid.grid, new Vector3Int(i, min.y - 1, k)));
                    if (ground.type != BlockType.ground)
                        return BuildingPlaceType.InvalidPlace;
                    if (BlockEx.GetShapeFromData(ground.data) != BlockShape.Full)
                        return BuildingPlaceType.InvalidPlace;

                    for (int j = min.y; j < max.y; j++)
                    {
                        var block = GridEx.GetBlock(grid.grid, GridEx.GetRealPosFromLoop(grid.grid, new Vector3Int(i, j, k)));
                        if (block.type != BlockType.air)
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

            if (GridEx.IntersectLoop(grid.grid, otherBounds, bounds))
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
                Event<HealEvent>.Broadcast(new HealEvent(Global.instance.buildingDatas.regenSpeed * Time.deltaTime, true), gameObject);

                var life = Event<GetLifeEvent>.Broadcast(new GetLifeEvent(), gameObject);
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

    protected virtual void SetComponentsEnabled(bool enabled)
    {
        var light = GetComponent<CustomLight>();
        if (light != null)
            light.enabled = enabled;
    }

    public void DisplayGenericInfos(UIElementContainer container)
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

    public void SetRotation(Rotation rot)
    {
        if (m_meshComponent != null)
            m_meshComponent.localRotation = RotationEx.ToQuaternion(rot);

        m_rotation = rot;
    }

    public Rotation GetRotation()
    {
        return m_rotation;
    }

    public virtual void UpdateRotation() { }

    public JsonObject Save()
    {
        var obj = new JsonObject();

        obj.AddElement("type", GetBuildingType().ToString());
        obj.AddElement("team", GetTeam().ToString());
        obj.AddElement("rot", GetRotation().ToString());

        var pos = transform.localPosition;
        obj.AddElement("pos", Json.FromVector3Int(new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z))));

        SaveImpl(obj);

        Event<SaveEvent>.Broadcast(new SaveEvent(obj), gameObject);

        return obj;
    }

    public static BuildingBase Create(JsonObject obj)
    {
        var typeJson = obj.GetElement("type");
        if (typeJson == null || !typeJson.IsJsonString())
            return null;

        BuildingType type;
        if (!Enum.TryParse<BuildingType>(typeJson.String(), out type))
            return null;

        var buildingData = Global.instance.buildingDatas.GetBuilding(type);
        if (buildingData == null || buildingData.prefab == null)
            return null;

        var instance = Instantiate(buildingData.prefab);
        instance.transform.parent = BuildingList.instance.transform;
        var building = instance.GetComponent<BuildingBase>();
        if(building == null)
        {
            Destroy(instance);
            return null;
        }

        building.Load(obj);

        return building;
    }

    public void Load(JsonObject obj)
    {
        var posJson = obj.GetElement("pos");
        if (posJson != null && posJson.IsJsonArray())
            transform.localPosition = Json.ToVector3Int(posJson.JsonArray());

        var teamJson = obj.GetElement("team");
        if (teamJson != null && teamJson.IsJsonString())
        {
            Team team;
            if (Enum.TryParse<Team>(teamJson.String(), out team))
                SetTeam(team);
        }
        var rotJson = obj.GetElement("rot");
        if (rotJson != null && rotJson.IsJsonString())
        {
            Rotation rot;
            if (Enum.TryParse<Rotation>(rotJson.String(), out rot))
            {
                SetRotation(rot);
                UpdateRotation();
            }
        }

        LoadImpl(obj);
        Event<LoadEvent>.Broadcast(new LoadEvent(obj), gameObject);
    }

    protected virtual void LoadImpl(JsonObject obj) { }

    protected virtual void SaveImpl(JsonObject obj) { }
}

