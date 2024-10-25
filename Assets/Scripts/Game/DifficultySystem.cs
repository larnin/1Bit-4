using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DifficultySystem : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    bool m_started = false;

    int m_nbKill = 0;
    int m_nbSpawnerDestroyed = 0;
    float m_maxDistance = 0;
    float m_time = 0;
    float m_maxDifficulty = 0;
    int m_nbSpawnerToSpawn = 0;

    static DifficultySystem m_instance = null;
    public static DifficultySystem instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;

        m_subscriberList.Add(new Event<OnKillEvent>.Subscriber(OnKill));
        m_subscriberList.Add(new Event<OnSpawnerDestroyEvent>.Subscriber(OnSpawnerDestroy));
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnEndGeneration));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;

        m_subscriberList.Unsubscribe();
    }

    void OnKill(OnKillEvent e)
    {
        m_nbKill++;
    }

    void OnSpawnerDestroy(OnSpawnerDestroyEvent e)
    {
        m_nbSpawnerDestroyed++;
    }

    void OnEndGeneration(GenerationFinishedEvent e)
    {
        m_started = true;
    }

    private void Update()
    {
        if (m_started)
            m_time += Time.deltaTime;

        if (ConnexionSystem.instance != null)
        {
            GetGridEvent grid = new GetGridEvent();
            Event<GetGridEvent>.Broadcast(grid);
            if (grid.grid != null)
            {
                int size = GridEx.GetRealSize(grid.grid);
                Vector2 center = new Vector2(size / 2.0f, size / 2.0f);

                int nbConnexions = ConnexionSystem.instance.GetConnectedBuildingNb();
                for (int i = 0; i < nbConnexions; i++)
                {
                    var building = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);
                    var pos = building.GetGroundCenter();

                    var posFromCenter = new Vector2(pos.x, pos.z) - center;
                    float dist = posFromCenter.magnitude;
                    if (dist > m_maxDistance)
                        m_maxDistance = dist;
                }
            }
        }

        float newDifficulty = GetDifficulty();
        if(newDifficulty > m_maxDifficulty)
        {
            int oldNbSpawner = Mathf.FloorToInt(Global.instance.difficultyDatas.difficultyToSpawnerNb.Get(m_maxDifficulty));
            int newNbSpawner = Mathf.FloorToInt(Global.instance.difficultyDatas.difficultyToSpawnerNb.Get(newDifficulty));

            if (newNbSpawner > oldNbSpawner)
                m_nbSpawnerToSpawn += newNbSpawner - oldNbSpawner;

            m_maxDifficulty = newDifficulty;
        }

        if (m_nbSpawnerToSpawn > 0)
            TrySpawnSpawner();
            
    }

    float GetDifficulty()
    {
        float difficultyPerMinute = Global.instance.difficultyDatas.difficultyPerMinute.Get(m_time / 60);
        float difficultyPerDistance = Global.instance.difficultyDatas.difficultyPerMinute.Get(m_maxDistance);
        float difficultyPerKill = Global.instance.difficultyDatas.difficultyPerMinute.Get(m_nbKill);
        float difficultyPerSpawner = Global.instance.difficultyDatas.difficultyPerMinute.Get(m_nbSpawnerDestroyed);

        float difficulty = difficultyPerMinute + difficultyPerDistance + difficultyPerKill + difficultyPerSpawner;
        if (difficulty < 0)
            return 0;
        return difficulty;
    }

    void TrySpawnSpawner()
    {

    }
}
