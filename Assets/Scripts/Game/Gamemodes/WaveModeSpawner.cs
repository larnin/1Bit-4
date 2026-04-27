using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public class WaveModeSpawner
{
    WaveMode m_mode;
    int m_portalIndex;

    bool m_spawnerAppeared = false;
    BuildingEnnemySpawner m_spawner = null;

    List<int> m_entitiesToSpawn = new List<int>();
    int m_waveIndex = 0;
    int m_entityIndex = 0;
    float m_delayToNextEntity = 0;

    public WaveModeSpawner(WaveMode mode, int portalIndex)
    {
        m_mode = mode;
        m_portalIndex = portalIndex;
    }

    public void StartAppear(float timeBeforeWave, int currentWave)
    {
        if (m_spawnerAppeared)
            return;

        var asset = m_mode.GetWaveAsset();
        if (asset == null || asset.portals.Count <= m_portalIndex)
            return;

        var prefab = GetPortalPrefab();
        if (prefab == null)
            return;
        var spawner = prefab.GetComponent<BuildingEnnemySpawner>();
        if (spawner == null)
            return;

        float delay = spawner.GetAppearDuration();

        int appearWave = asset.portals[m_portalIndex].waveStart;

        if (currentWave == appearWave && timeBeforeWave <= delay)
            SpawnPortal();
        if (currentWave > appearWave)
            SpawnPortal();
    }

    public void StartWave(int index)
    {
        var asset = m_mode.GetWaveAsset();
        if (asset == null || asset.portals.Count <= m_portalIndex)
            return;

        int localIndex = index - asset.portals[m_portalIndex].waveStart;
        if (localIndex < 0 || localIndex >= asset.portals[m_portalIndex].waves.Count)
            return;

        m_entitiesToSpawn.Clear();
        m_entityIndex = 0;
        m_waveIndex = localIndex;

        for(int i = 0; i < asset.portals[m_portalIndex].waves[localIndex].entities.Count; i++)
        {
            int count = asset.portals[m_portalIndex].waves[localIndex].entities[i].count;
            for (int j = 0; j < count; j++)
                m_entitiesToSpawn.Add(i);
        }

        m_entitiesToSpawn.Shuffle(StaticRandomGenerator<MT19937>.Get());

        m_delayToNextEntity = asset.portals[m_portalIndex].waves[localIndex].delayBeforeFirstEntity;
    }

    public bool ProcessWave(float deltaTime)
    {
        if (!m_spawnerAppeared)
            return true;

        if (m_spawner != null && !m_spawner.HaveAppeared())
            return false;

        if (m_entityIndex >= m_entitiesToSpawn.Count())
            return true;

        var asset = m_mode.GetWaveAsset();
        if (asset == null)
            return true;

        if (m_waveIndex < 0 || m_waveIndex >= asset.portals[m_portalIndex].waves.Count)
            return true;

        m_delayToNextEntity -= deltaTime;
        while(m_delayToNextEntity <= 0)
        {
            SpawnOneEntity(m_entityIndex);
            m_entityIndex++;
            if (m_entitiesToSpawn.Count > 1)
                m_delayToNextEntity += asset.portals[m_portalIndex].waves[m_waveIndex].spawnDuration / (m_entitiesToSpawn.Count - 1);
            if (m_entityIndex >= m_entitiesToSpawn.Count)
                return true;
        }

        return false;
    }

    public void OnEnd()
    {

    }

    GameObject GetPortalPrefab()
    {
        if (m_mode == null)
            return null;

        var asset = m_mode.GetWaveAsset();
        if (asset == null)
            return null;

        if (m_portalIndex < 0 || m_portalIndex >= asset.portals.Count)
            return null;

        var buildingInfo = Global.instance.buildingDatas.GetBuilding(asset.portals[m_portalIndex].portalType);
        if (buildingInfo == null)
            return null;

        return buildingInfo.prefab;
    }

    void SpawnPortal()
    {
        var asset = m_mode.GetWaveAsset();
        if (asset == null)
            return;

        if (m_portalIndex < 0 || m_portalIndex >= asset.portals.Count)
            return;

        if (QuestElementList.instance == null)
            return;

        var obj = QuestElementList.instance.GetFirstNamedObjectByName(asset.portals[m_portalIndex].positionName);
        if (obj == null)
            return;

        var pos = obj.transform.position;
        Vector3Int groundPos = new Vector3Int(Mathf.RoundToInt(pos.x), -1, Mathf.RoundToInt(pos.z));

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent()).grid;
        if (grid == null)
            return;

        groundPos.y = GridEx.GetHeight(grid, new Vector2Int(groundPos.x, groundPos.z)) + 1;
        if (groundPos.y <= 0)
            return;

        var spawner = Global.instance.buildingDatas.GetBuilding(asset.portals[m_portalIndex].portalType);
        if (spawner == null)
            return;

        var instance = GameObject.Instantiate(spawner.prefab);
        instance.transform.parent = BuildingList.instance.transform;
        instance.transform.position = groundPos;

        m_spawner = instance.GetComponent<BuildingEnnemySpawner>();
        if (m_spawner == null)
            GameObject.Destroy(instance);
        m_spawnerAppeared = true;
    }

    void SpawnOneEntity(int index)
    {
        if (index < 0 || index >= m_entitiesToSpawn.Count)
            return;
        int entityIndex = m_entitiesToSpawn[index];

        var asset = m_mode.GetWaveAsset();
        if (asset == null)
            return;

        if (m_portalIndex < 0 || m_portalIndex >= asset.portals.Count)
            return;

        if (m_waveIndex < 0 || m_waveIndex >= asset.portals[m_portalIndex].waves.Count)
            return;

        if (entityIndex < 0 || entityIndex >= asset.portals[m_portalIndex].waves[m_waveIndex].entities.Count)
            return;

        string entityName = asset.portals[m_portalIndex].waves[m_waveIndex].entities[entityIndex].entityType.GetValue();

        var prefab = Global.instance.editorDatas.GetEntityPrefab(entityName);
        if (prefab == null)
            return;

        var stratPos = m_spawner.GetGroundCenter();

        var obj = GameObject.Instantiate(prefab);
        if (EntityList.instance != null)
            obj.transform.parent = EntityList.instance.transform;
        obj.transform.position = stratPos;

        foreach(var s in asset.portals[m_portalIndex].waves[m_waveIndex].entities[entityIndex].stats)
            Event<AddStatEvent>.Broadcast(new AddStatEvent(s.stat, s.value, "gamemode"), obj);
    }
}
