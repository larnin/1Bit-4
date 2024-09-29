using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    enum State
    {
        Starting,
        GeneratingWorld,
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
        WorldGenerator.Generate(Global.instance.generatorSettings);
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
                        m_state = State.RenderingWorld;
                    }
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
}
