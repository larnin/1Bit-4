using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRand;
using UnityEngine;

public class MonolithModeSpawner
{
    MonolithMode m_mode;
    BuildingBase m_building;

    bool m_haveAppeared = false;
    float m_timer = 0;

    List<int> m_entityIndexs = new List<int>();
    float m_spawnDeltaTime = 0;

    public MonolithModeSpawner(MonolithMode mode, BuildingBase building)
    {
        m_mode = mode;
        m_building = building;
    }

    public BuildingBase GetBuilding()
    {
        return m_building;
    }

    public void Update(float deltaTime)
    {
        if (!m_haveAppeared)
            UpdateAppear();
        else UpdateSpawning(deltaTime);
    }

    public void StartNextWave(float score)
    {
        if (!m_haveAppeared)
            return;

        var asset = m_mode.GetMonolithAsset();

        if (asset == null)
            return;

        List<float> weights = new List<float>();
        
        foreach(var e in asset.ennemies)
        {
            if (e.scoreMin > score || e.scoreMax < score)
                weights.Add(0);
            else weights.Add(e.weight);
        }

        var rand = StaticRandomGenerator<MT19937>.Get();

        while(score > 0)
        {
            int index = Rand.DiscreteDistribution(weights, rand);
            score -= asset.ennemies[index].cost;

            int entityIndex = Global.instance.editorDatas.GetEntityIndex(asset.ennemies[index].entityType.GetValue());
            if (entityIndex >= 0)
                m_entityIndexs.Add(entityIndex);
        }

        int nbEntities = m_entityIndexs.Count;

        if (nbEntities > 0)
            m_spawnDeltaTime = asset.spawnerSpawnDuration / nbEntities;
        else m_spawnDeltaTime = 1;

        m_timer = m_spawnDeltaTime;
    }

    void UpdateAppear()
    {
        BuildingEnnemySpawner Spawner = m_building as BuildingEnnemySpawner;
        if (Spawner == null)
            return;
        if (Spawner.HaveAppeared())
            m_haveAppeared = true;
    }

    void UpdateSpawning(float deltaTime)
    {
        m_timer += deltaTime;

        int toSpawn = (int)(m_timer / m_spawnDeltaTime);
        while(toSpawn --> 0)
        {
            if(m_entityIndexs.Count > 0)
            {
                SpawnOneEntity(m_entityIndexs[0]);
                m_entityIndexs.RemoveAt(0);
            }
        }
        m_timer -= toSpawn * m_spawnDeltaTime;
    }

    void SpawnOneEntity(int entityIndex)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        if (entityIndex < 0 || entityIndex >= Global.instance.editorDatas.entities.Count)
            return;
        var prefab = Global.instance.editorDatas.entities[entityIndex].prefab;

        if (grid.grid == null || prefab == null)
            return;

        var pos = m_building.transform.position;

        var posInt = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        int height = GridEx.GetHeight(grid.grid, posInt);

        var obj = GameObject.Instantiate(prefab);
        if (EntityList.instance != null)
            obj.transform.parent = EntityList.instance.transform;
        obj.transform.position = new Vector3(posInt.x, height + 1, posInt.y);
    }
}