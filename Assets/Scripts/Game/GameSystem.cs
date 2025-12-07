using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum EntityType
{
    None,
    Building,
    GameEntity,
    Projectile,
    Resource,
    Quest
}

public class GameSystem : MonoBehaviour
{
    enum State
    {
        Starting,
        GeneratingWorld,
        PlaceTower,
        LoadingWorld,
        RenderingWorld,
        Ended
    }

    [SerializeField] GridBehaviour m_grid;
    [SerializeField] List<BuildingType> m_unlockedBuilding;

    State m_state = State.Starting;
    float m_delay;
    float m_alarmTimer = 0;

    SubscriberList m_subscriberList = new SubscriberList();

    static GameSystem m_instance = null;
    public static GameSystem instance { get { return m_instance; } }

    public List<BuildingType> GetUnlockedBuildings()
    {
        return m_unlockedBuilding;
    }

    public void SetBuildingUnlocked(BuildingType type, bool unlocked)
    {
        if(unlocked && ! m_unlockedBuilding.Contains(type))
            m_unlockedBuilding.Add(type);
        if (!unlocked)
            m_unlockedBuilding.Remove(type);
    }

    private void Awake()
    {
        m_instance = this;

        m_subscriberList.Add(new Event<TowerDeathEvent>.Subscriber(OnTowerDeath));
        m_subscriberList.Add(new Event<QuestEndLevelEvent>.Subscriber(OnQuestEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();

        if (m_instance == this)
            m_instance = null;
    }

    private void Update()
    {
        m_alarmTimer -= Time.deltaTime;

        switch (m_state)
        {
            case State.Starting:
                {
                    m_delay += Time.deltaTime;
                    if (m_delay >= 1)
                        StartLoadOrGeneratingWorld();
                    break;
                }
            case State.GeneratingWorld:
                {
                    if(WorldGenerator.GetState() == WorldGenerator.GenerationState.Finished)
                    {
                        var grid = WorldGenerator.GetGrid();
                        if(m_grid != null)
                            m_grid.SetGrid(grid);
                        Event<SetGridEvent>.Broadcast(new SetGridEvent(grid));
                        m_state = State.PlaceTower;
                    }
                    break;
                }
            case State.PlaceTower:
                {
                    PlaceTower();
                    m_state = State.RenderingWorld;
                    break;
                }
            case State.LoadingWorld:
                {
                    m_state = State.RenderingWorld;
                    break;
                }
            case State.RenderingWorld:
                {
                    int generatedChunks = 0; 
                    int totalChunks = 0; 
                    if(m_grid != null)
                    {
                        generatedChunks = m_grid.GetGeneratedCount();
                        totalChunks = m_grid.GetTotalCount();
                    }
                    else
                    {
                        var e = Event<GetGridGenerationStatusEvent>.Broadcast(new GetGridGenerationStatusEvent());
                        generatedChunks = e.generatedChunks;
                        totalChunks = e.totalChunks;
                    }

                    if (generatedChunks == totalChunks)
                    {
                        m_state = State.Ended;
                        StartMainQuest();
                        Event<GenerationFinishedEvent>.Broadcast(new GenerationFinishedEvent());
                    }

                    break;
                }
            default:
                break;
        }
    }

    void StartLoadOrGeneratingWorld()
    {
        if(GameInfos.instance.gameParams.infiniteMode)
        {
            m_state = State.GeneratingWorld;
            
            var preset = WorldGeneratorSettingsPreset.instance.GetPresetWithCopy(Global.instance.levelsData.InfiniteLevel.presetName);
            if (preset != null)
            {
                preset.size = Global.instance.levelsData.InfiniteLevel.size;
                preset.height = Global.instance.levelsData.InfiniteLevel.height;
                preset.loopX = Global.instance.levelsData.InfiniteLevel.loopX;
                preset.loopZ = Global.instance.levelsData.InfiniteLevel.loopZ;
                WorldGenerator.Generate(preset, GameInfos.instance.gameParams.seed);
            }
            else Debug.LogError("Invalid infinite mode preset");
        }
        else
        {
            m_state = State.LoadingWorld;

            var level = GameInfos.instance.gameParams.level.level;
            if (level.data != null)
            {
                JsonDocument doc = Json.ReadFromString(level.data);
                if (doc != null)
                {
                    var root = doc.GetRoot();
                    if (root == null || !root.IsJsonObject())
                        return;

                    SaveWorld.Load(root.JsonObject());
                }
                else Debug.LogError("Invalid Json on level loading");
            }
            else Debug.LogError("No level asset set on " + GameInfos.instance.gameParams.level.name);
        }
    }

    void PlaceTower()
    {
        if (BuildingList.instance == null)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        var size = GridEx.GetRealSize(grid.grid);

        Vector2Int centerPos = new Vector2Int(size / 2, size / 2);
        var buildingData = Global.instance.buildingDatas.GetBuilding(BuildingType.Tower);
        if (buildingData == null || buildingData.prefab == null)
            return;

        Vector3Int towerPos = SearchValidPos(grid.grid, centerPos, new Vector2Int(buildingData.size.x, buildingData.size.z), 5);

        var obj = Instantiate(buildingData.prefab);
        obj.transform.parent = BuildingList.instance.transform;
        obj.transform.localPosition = towerPos;
    }

    Vector3Int SearchValidPos(Grid grid, Vector2Int center, Vector2Int elementSize, int searchDistance)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        for(int i = -searchDistance; i <= searchDistance; i++)
        {
            for(int j = -searchDistance; j<= searchDistance; j++)
            {
                points.Add(new Vector2Int(i, j) + center);
            }
        }

        points.Sort((Vector2Int a, Vector2Int b) => { return (center - a).sqrMagnitude.CompareTo((center - b).sqrMagnitude); });

        foreach(var p in points)
        {
            if(IsPosValid(grid, p, elementSize))
            {
                var h = GridEx.GetHeight(grid, p);
                return new Vector3Int(p.x, h + 1, p.y);
            }
        }

        var h2 = GridEx.GetHeight(grid, center);
        return new Vector3Int(center.x, h2 + 1, center.y); ;
    }

    void StartMainQuest()
    {

    }

    bool IsPosValid(Grid grid, Vector2Int pos, Vector2Int elementSize)
    {
        Vector2Int minPos = new Vector2Int((elementSize.x - 1) / 2, (elementSize.y - 1) / 2);
        Vector2Int maxPos = pos - minPos + elementSize;
        minPos = pos - minPos;

        int lastHeight = -1;

        for(int i = minPos.x; i < maxPos.x; i++)
        {
            for(int j = minPos.y; j < maxPos.y; j++)
            {
                int height = GridEx.GetHeight(grid, new Vector2Int(i, j));
                if (height < 0)
                    continue;
                if (lastHeight < 0)
                    lastHeight = height;
                if (lastHeight != height)
                    return false;
            }
        }

        return lastHeight >= 0;
    }

    public static EntityType GetEntityType(GameObject obj)
    {
        if (obj.GetComponent<BuildingBase>() != null)
            return EntityType.Building;
        if (obj.GetComponent<GameEntity>() != null)
            return EntityType.GameEntity;
        if (obj.GetComponent<ProjectileBase>() != null)
            return EntityType.Projectile;
        if (obj.GetComponent<QuestElement>() != null)
            return EntityType.Quest;
        if (obj.GetComponent<BlockResourceDisplay>() != null)
            return EntityType.Resource;

        return EntityType.None;
    }

    public static JsonObject GetEntityData(GameObject obj)
    {
        var building = obj.GetComponent<BuildingBase>();
        if (building != null)
            return building.Save();
        var gameEntity = obj.GetComponent<GameEntity>();
        if (gameEntity != null)
            return gameEntity.Save();
        var projectile = obj.GetComponent<ProjectileBase>();
        if (projectile != null)
            return projectile.Save();
        var questElm = obj.GetComponent<QuestElement>();
        if(questElm != null)
            return questElm.Save();

        return null;
    }

    public string GetStatus()
    {
        switch (m_state)
        {
            case State.Starting:
                return "";
            case State.GeneratingWorld:
                return WorldGenerator.GetStateText();
            case State.PlaceTower:
                return "Place Tower";
            case State.RenderingWorld:
                int generatedChunks = 0;
                int totalChunks = 0;
                if (m_grid != null)
                {
                    generatedChunks = m_grid.GetGeneratedCount();
                    totalChunks = m_grid.GetTotalCount();
                }
                else
                {
                    var e = Event<GetGridGenerationStatusEvent>.Broadcast(new GetGridGenerationStatusEvent());
                    generatedChunks = e.generatedChunks;
                    totalChunks = e.totalChunks;
                }
                return "Rendering world - " + (generatedChunks * 100 / totalChunks).ToString() + "%";
            default:
                return "Ready";
        }
    }

    public bool IsLoaded()
    {
        return m_state == State.Ended;
    }

    public void StartAlarm()
    {
        if (m_alarmTimer > 0)
            return;

        m_alarmTimer = Global.instance.buildingDatas.alarmGlobalRestartDelay;

        if (SoundSystem.instance != null)
            SoundSystem.instance.PlaySoundUI(Global.instance.buildingDatas.alarmSound, Global.instance.buildingDatas.alarmVolume);
    }

    void OnTowerDeath(TowerDeathEvent e)
    {
        TriggerEndLevel(false);
    }

    void OnQuestEnd(QuestEndLevelEvent e)
    {
        TriggerEndLevel(e.succes);
    }

    void TriggerEndLevel(bool succes)
    {
        DisplayEndLevelEvent displayEnd = Event<DisplayEndLevelEvent>.Broadcast(new DisplayEndLevelEvent(succes));
        if(!displayEnd.displayed)
        {
            if (MenuSystem.instance == null)
                return;

            var menu = MenuSystem.instance.OpenMenu<GenericGameOverMenu>("GenericGameOver");
            menu.SetStatus(succes);
        }
    }
}
