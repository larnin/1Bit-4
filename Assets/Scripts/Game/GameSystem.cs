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
    Ennemy,
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

    private void Start()
    {
        WorldGenerator.Generate(Global.instance.GetWorldGeneratorSettings(GameInfos.instance.gameParams.worldSize), GameInfos.instance.gameParams.seed);
        m_state = State.GeneratingWorld;
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

        Vector3Int towerPos = new Vector3Int(size / 2, 0, size / 2);
        towerPos.y = GridEx.GetHeight(grid, new Vector2Int(towerPos.x, towerPos.z)) + 1;

        var buildingData = Global.instance.buildingDatas.GetBuilding(BuildingType.Tower);
        if (buildingData == null || buildingData.prefab == null)
            return;

        var obj = Instantiate(buildingData.prefab);
        obj.transform.parent = BuildingList.instance.transform;
        obj.transform.localPosition = towerPos;
    }

    public static EntityType GetEntityType(GameObject obj)
    {
        if (obj.GetComponent<BuildingBase>() != null)
            return EntityType.Building;
        if (obj.GetComponent<EnnemyEntity>() != null)
            return EntityType.Ennemy;
        if (obj.GetComponent<ProjectileBase>() != null)
            return EntityType.Projectile;

        return EntityType.None;
    }

    public string GetStatus()
    {
        switch (m_state)
        {
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
}
