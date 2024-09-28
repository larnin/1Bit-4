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

    State m_state = State.Starting;

    private void Start()
    {
        WorldGenerator.Generate(Global.instance.generatorSettings);
        m_state = State.GeneratingWorld;
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
