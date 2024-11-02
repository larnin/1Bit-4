﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

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

    List<EnnemySpawner> m_spawners = new List<EnnemySpawner>();

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

    public float GetDifficulty()
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
        if (ConnexionSystem.instance == null)
            return;

        int nbBuilding = ConnexionSystem.instance.GetConnectedBuildingNb();

        var rand = StaticRandomGenerator<MT19937>.Get();

        for(int i = 0; i < 10; i++)
        {
            var buildingIndex = Rand.UniformIntDistribution(nbBuilding, rand);
            var building = ConnexionSystem.instance.GetConnectedBuildingFromIndex(buildingIndex);

            var offsetDistance = Rand.UniformFloatDistribution(rand);
            offsetDistance = offsetDistance * Global.instance.difficultyDatas.spawnersData.distanceFromBuildingsMin + (1 - offsetDistance) * Global.instance.difficultyDatas.spawnersData.distanceFromBuildingsMax;
            var offset = Rand2D.UniformVector2CircleSurfaceDistribution(rand) * offsetDistance;

            var pos = building.GetGroundCenter() + new Vector3(offset.x, 0, offset.y);
            var posI = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

            bool distOk = true;
            for(int j = 0; j < nbBuilding; j++)
            {
                Vector3 buildingPos = ConnexionSystem.instance.GetConnectedBuildingFromIndex(j).GetGroundCenter();

                float dist = (buildingPos - posI).SqrMagnitudeXZ();
                if(dist < Global.instance.difficultyDatas.spawnersData.distanceFromBuildingsMin * Global.instance.difficultyDatas.spawnersData.distanceFromBuildingsMin)
                {
                    distOk = false;
                    break;
                }
            }

            if(distOk)
            {
                var prefab = Global.instance.difficultyDatas.spawnersData.prefab;
                if(prefab != null)
                {
                    var obj = Instantiate(prefab);
                    obj.transform.parent = transform;
                    obj.transform.position = posI;
                }

                m_nbSpawnerToSpawn--;

                return;
            }
        }
    }

    public void Register(EnnemySpawner spawner)
    {
        m_spawners.Add(spawner);
    }

    public void UnRegister(EnnemySpawner spawner)
    {
        m_spawners.Remove(spawner);
    }

    public int GetSpawnerNb()
    {
        return m_spawners.Count();
    }

    public EnnemySpawner GetSpawnerFromIndex(int index)
    {
        if (index < 0 || index >= m_spawners.Count)
            return null;
        return m_spawners[index];
    }

    public EnnemySpawner GetNearestSpawner(Vector3 pos)
    {
        float bestDist = 0;
        EnnemySpawner bestSpawner = null;

        foreach (var s in m_spawners)
        {
            float dist = (pos - s.transform.position).sqrMagnitude;

            if (dist < bestDist || bestSpawner == null)
            {
                bestDist = dist;
                bestSpawner = s;
            }
        }

        return bestSpawner;
    }
}