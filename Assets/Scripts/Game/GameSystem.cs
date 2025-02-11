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
    Entity,
    Projectile,
}

public class GameSystem : MonoBehaviour
{
    enum State
    {
        Starting,
        GeneratingWorld,
        PlaceTower,
        RenderingWorld,
        Ended
    }

    [SerializeField] GridBehaviour m_grid;
    [SerializeField] List<BuildingType> m_unlockedBuilding;

    State m_state = State.Starting;
    float m_delay;

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
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    private void Update()
    {
        switch(m_state)
        {
            case State.Starting:
                {
                    m_delay += Time.deltaTime;
                    if(m_delay >= 1)
                    {
                        WorldGenerator.Generate(Global.instance.GetWorldGeneratorSettings(GameInfos.instance.gameParams.worldSize), GameInfos.instance.gameParams.seed);
                        m_state = State.GeneratingWorld;
                    }
                    break;
                }
            case State.GeneratingWorld:
                {
                    if(WorldGenerator.GetState() == WorldGenerator.GenerationState.Finished)
                    {
                        var grid = WorldGenerator.GetGrid();
                        m_grid.SetGrid(grid);
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
            case State.RenderingWorld:
                {
                    int generatedChunks = m_grid.GetGeneratedCount();
                    int totalChunks = m_grid.GetTotalCount();

                    if (generatedChunks == totalChunks)
                    {
                        m_state = State.Ended;
                        Event<GenerationFinishedEvent>.Broadcast(new GenerationFinishedEvent());
                    }

                    break;
                }
            default:
                break;
        }
    }

    void PlaceTower()
    {
        if (BuildingList.instance == null)
            return;

        var grid = m_grid.GetGrid();
        var size = GridEx.GetRealSize(grid);

        Vector2Int centerPos = new Vector2Int(size / 2, size / 2);
        var buildingData = Global.instance.buildingDatas.GetBuilding(BuildingType.Tower);
        if (buildingData == null || buildingData.prefab == null)
            return;

        Vector3Int towerPos = SearchValidPos(grid, centerPos, new Vector2Int(buildingData.size.x, buildingData.size.z), 5);

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
            return EntityType.Entity;
        if (obj.GetComponent<ProjectileBase>() != null)
            return EntityType.Projectile;

        return EntityType.None;
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
                int generatedChunks = m_grid.GetGeneratedCount();
                int totalChunks = m_grid.GetTotalCount();
                return "Rendering world - " + (generatedChunks * 100 / totalChunks).ToString() + "%";
            default:
                return "Ready";
        }
    }

    public bool IsLoaded()
    {
        return m_state == State.Ended;
    }
}
