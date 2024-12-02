using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingEnnemySpawner : BuildingBase
{
    public override BuildingType GetBuildingType()
    {
        return BuildingType.EnnemySpawner;
    }

    enum State
    {
        Starting,
        Waiting,
        Spawning,
    }

    float m_timer;
    float m_deltaTime;
    State m_state = State.Starting;
    List<int> m_entityIndexs = new List<int>();
    int m_currentIndex = 0;

    protected override void Update()
    {
        base.Update();

        switch (m_state)
        {
            case State.Starting:
                StartNextWait();
                break;
            case State.Waiting:
                m_timer -= Time.deltaTime;
                if (m_timer < 0)
                    StartNextWave();
                break;
            case State.Spawning:
                ProcessSpawning();
                break;
        }
    }

    void StartNextWait()
    {
        float min = Global.instance.difficultyDatas.spawnersData.delayBetweenWavesMin;
        float max = Global.instance.difficultyDatas.spawnersData.delayBetweenWavesMax;

        float time = Rand.UniformFloatDistribution(min, max, StaticRandomGenerator<MT19937>.Get());

        m_timer = time;
        m_state = State.Waiting;
    }

    void StartNextWave()
    {
        if (DifficultySystem.instance == null)
        {
            StartNextWait();
            return;
        }

        float difficulty = DifficultySystem.instance.GetDifficulty();

        List<int> allowedIndexs = new List<int>();
        for (int i = 0; i < Global.instance.difficultyDatas.spawnersData.ennemies.Count; i++)
        {
            var e = Global.instance.difficultyDatas.spawnersData.ennemies[i];

            bool min = e.difficultyMin < 0 || e.difficultyMin <= difficulty;
            bool max = e.difficultyMax < 0 || e.difficultyMax >= difficulty;

            if (min && max)
                allowedIndexs.Add(i);
        }

        if (allowedIndexs.Count == 0)
        {
            StartNextWait();
            return;
        }

        List<float> weights = new List<float>();
        foreach (var index in allowedIndexs)
            weights.Add(Global.instance.difficultyDatas.spawnersData.ennemies[index].weight);

        m_entityIndexs.Clear();
        m_currentIndex = 0;
        m_timer = 0;
        m_state = State.Spawning;

        var rand = StaticRandomGenerator<MT19937>.Get();

        while (difficulty > 0)
        {
            var tabIndex = Rand.DiscreteDistribution(weights, rand);
            var index = allowedIndexs[tabIndex];
            m_entityIndexs.Add(index);

            var e = Global.instance.difficultyDatas.spawnersData.ennemies[index];
            difficulty -= Rand.UniformFloatDistribution(e.difficultyCostMin, e.difficultyCostMax, rand);
        }

        m_entityIndexs.Shuffle(rand);

        m_deltaTime = Global.instance.difficultyDatas.spawnersData.delayBaseBetweenEnnemies;
        int nbMore = m_entityIndexs.Count - Global.instance.difficultyDatas.spawnersData.delayReduceAfterNbEnnemies;
        if (nbMore > 0)
        {
            float multiplier = 1 + nbMore * Global.instance.difficultyDatas.spawnersData.delaySpeedMultiplayerPerEnnemies;
            m_deltaTime /= multiplier;
        }
    }

    void ProcessSpawning()
    {
        m_timer += Time.deltaTime;
        while (m_timer >= m_deltaTime)
        {
            m_timer -= m_deltaTime;

            if (m_currentIndex >= m_entityIndexs.Count)
            {
                StartNextWait();
                return;
            }

            int index = m_entityIndexs[m_currentIndex];
            SpawnOneEnnemie(index);
            m_currentIndex++;
        }

        if (m_currentIndex >= m_entityIndexs.Count)
        {
            StartNextWait();
            return;
        }
    }

    void SpawnOneEnnemie(int index)
    {
        GetGridEvent grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);

        var e = Global.instance.difficultyDatas.spawnersData.ennemies[index];

        if (grid.grid == null || e.prefab == null)
            return;

        var pos = transform.position;

        var posInt = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        int height = GridEx.GetHeight(grid.grid, posInt);

        var obj = Instantiate(e.prefab);
        obj.transform.parent = transform;
        obj.transform.position = new Vector3(posInt.x, height + 1, posInt.y);
    }
}

