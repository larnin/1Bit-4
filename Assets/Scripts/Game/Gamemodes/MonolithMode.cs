using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MonolithMode : GamemodeBase
{
    class BuildingStatus
    {
        public BuildingMonolith building;
        public float m_timer = 0;
        public List<MonolithModeSpawner> m_spawners = new List<MonolithModeSpawner>();
    }

    class NewSpawnerData
    {
        public BuildingStatus source;
        public int retryCount = 0;
    }

    List<BuildingStatus> m_aliveBuildings = new List<BuildingStatus>();
    int m_StartBuildingNb = 0;

    MonolithModeAsset m_asset;

    SubscriberList m_subscriberList;

    float m_currentScore = 0;

    List<NewSpawnerData> m_newSpawners = new List<NewSpawnerData>();

    public MonolithMode(MonolithModeAsset asset, GamemodeSystem owner)
        : base(owner)
    {
        m_asset = asset;
        m_currentScore = m_asset.initialScore;
    }

    public override GamemodeAssetBase GetAsset()
    {
        return m_asset;
    }

    public MonolithModeAsset GetMonolithAsset()
    {
        return m_asset;
    }

    public override GamemodeStatus GetStatus()
    {
        if (m_aliveBuildings.Count == 0)
            return GamemodeStatus.Completed;

        return GamemodeStatus.Ongoing;
    }

    public override void Begin()
    { 
        if(m_subscriberList == null)
        {
            m_subscriberList = new SubscriberList();
            m_subscriberList.Add(new Event<OnBuildingDamagedEvent>.Subscriber(OnMonilithDamaged));
            m_subscriberList.Add(new Event<OnBuildingDestroyEvent>.Subscriber(OnBuildingDestroyed));
        }

        m_subscriberList.Subscribe();

        if(BuildingList.instance != null)
        {
            var buildings = BuildingList.instance.GetAllBuilding(BuildingType.Monolith, Team.Ennemy);
            foreach(var b in buildings)
            {
                if (b is BuildingMonolith)
                {
                    BuildingStatus building = new BuildingStatus();
                    building.building = b as BuildingMonolith;
                    m_aliveBuildings.Add(building);
                }
                    
            }
        }
        m_StartBuildingNb = m_aliveBuildings.Count;
    }

    public override void Process()
    {
        if (GameInfos.instance.paused)
            return;

        float deltaTime = Time.deltaTime;

        UpdateAliveMonoliths();
        foreach(var b in m_aliveBuildings)
            UpdateAngryBuilding(b, deltaTime);

        UpdateNewSpawners();
    }

    public override void End() 
    {
        m_subscriberList.Unsubscribe();
    }

    void OnMonilithDamaged(OnBuildingDamagedEvent e)
    {

    }

    void OnBuildingDestroyed(OnBuildingDestroyEvent e)
    {

    }

    public void TriggerMonolith(BuildingMonolith building)
    {

    }

    void UpdateAngryBuilding(BuildingStatus building, float deltaTime)
    {
        if (building.building == null)
            return;

        if (building.building.GetState() != BuildingMonolith.State.AngryLoop)
            return;

        building.m_timer += deltaTime;

        if(building.m_timer >= m_asset.delayBetweenWave)
        {
            StartWaveFromBuilding(building);
            building.m_timer = 0;
        }
    }

    void UpdateAliveMonoliths()
    {
        List<BuildingStatus> toRemove = new List<BuildingStatus>();

        foreach(var m in m_aliveBuildings)
        {
            if(m.building == null)
            {
                toRemove.Add(m);
                continue;
            }
            if(Event<IsDeadEvent>.Broadcast(new IsDeadEvent()).isDead)
            {
                toRemove.Add(m);
                continue;
            }
        }

        foreach(var r in toRemove)
        {
            m_aliveBuildings.Remove(r);

            foreach(var s in r.m_spawners)
            {
                if (s.GetBuilding() == null)
                    continue;

                var life = Event<GetLifeEvent>.Broadcast(new GetLifeEvent());
                Event<HitEvent>.Broadcast(new HitEvent(new Hit(life.maxLife * 10000)), s.GetBuilding().gameObject);
            }
        }
    }

    void StartWaveFromBuilding(BuildingStatus building)
    {
        foreach(var spawner in building.m_spawners)
        {
            spawner.StartNextWave(m_currentScore);
        }

        int total = GetTotalSpawners();
        int maxSpawner = GetMaxAllowedSpawners();

        int spawnCount = m_asset.spawnerPerWave;
        if (spawnCount > maxSpawner - total)
            spawnCount = maxSpawner - total;

        for(int i = 0; i < spawnCount; i++)
        {
            var spawnerData = new NewSpawnerData();
            spawnerData.source = building;
            m_newSpawners.Add(spawnerData);
        }
    }

    int GetTotalSpawners()
    {
        int count = 0;
        foreach (var monolith in m_aliveBuildings)
            count += monolith.m_spawners.Count;

        count += m_newSpawners.Count;

        return count;
    }

    int GetMaxAllowedSpawners()
    {
        int angryCount = 0;
        foreach (var monolith in m_aliveBuildings)
        {
            if (monolith.building == null)
                continue;
            if (monolith.building.GetState() != BuildingMonolith.State.Idle)
                angryCount++;
        }

        if (angryCount == 0)
            return 0;

        return m_asset.spawnerMaxNb + m_asset.spawnerIncreaseMaxPerAngryMonolith * (angryCount - 1);
    }

    void UpdateNewSpawners()
    {
        const int maxRetryCount = 20;

        List<NewSpawnerData> toRemoveList = new List<NewSpawnerData>();

        foreach(var s in m_newSpawners)
        {
            if(s.source == null)
            {
                toRemoveList.Add(s);
                continue;
            }

            Vector3Int pos;
            if(!GetValidSpawnerPosition(s.source, out pos))
            {
                s.retryCount++;
                if (s.retryCount > maxRetryCount)
                    toRemoveList.Add(s);
                continue;
            }

            SpawnSpawner(s, pos);
            toRemoveList.Add(s);
        }

        foreach (var r in toRemoveList)
            m_newSpawners.Remove(r);
    }

    void SpawnSpawner(NewSpawnerData data, Vector3Int pos)
    {
        if (data.source == null)
            return;

        var spawner = Global.instance.buildingDatas.GetBuilding(BuildingType.EnnemySpawner);
        if (spawner != null)
        {
            var obj = GameObject.Instantiate(spawner.prefab);
            obj.transform.parent = BuildingList.instance.transform;
            obj.transform.position = pos;

            var spawnerInfos = new MonolithModeSpawner(this, obj.GetComponent<BuildingBase>());
            if(spawnerInfos.GetBuilding() == null)
                Debug.LogError("No building in the prefab " + spawner.prefab.name);

            data.source.m_spawners.Add(spawnerInfos);
        }
    }

    bool GetValidSpawnerPosition(BuildingStatus source, out Vector3Int pos)
    {
        pos = Vector3Int.zero;

        var spawner = Global.instance.buildingDatas.GetBuilding(BuildingType.EnnemySpawner);
        if (spawner == null || spawner.prefab == null)
            return false;

        if (source == null || source.building == null)
            return false;

        if (BuildingList.instance == null)
            return false;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return false;

        int size = GridEx.GetRealSize(grid.grid);

        var gen = StaticRandomGenerator<MT19937>.Get();

        //get a valid position around the monolith

        Vector3 center = source.building.GetGroundCenter();
        Vector2 testPos2 = new Vector2(center.x, center.z) + Rand2D.UniformVector2CircleDistribution(m_asset.spawnerRadiusAroundMonolith, gen);
        Vector3Int realPos = new Vector3Int(Mathf.RoundToInt(testPos2.x), 0, Mathf.RoundToInt(testPos2.y));
        Vector3Int realPosLoop = GridEx.GetRealPosFromLoop(grid.grid, new Vector3Int(realPos.x, 0, realPos.z));
        if (!grid.grid.LoopX() && realPos.x != realPosLoop.x)
            return false;
        if (!grid.grid.LoopZ() && realPos.z != realPosLoop.z)
            return false;

        realPosLoop.y = GridEx.GetHeight(grid.grid, new Vector2Int(realPosLoop.x, realPosLoop.z));
        if (realPosLoop.y < 0)
            return false;
        realPosLoop.y++;

        //invalid position if ground is not flat or on building
        
        Vector3Int buildingMin = spawner.size / 2;
        buildingMin.y = 0;

        for(int i = 0; i < spawner.size.x; i++)
        {
            for(int j = 0; j < spawner.size.z; j++)
            {
                Vector3Int testPoint = new Vector3Int(i, 0, j) + buildingMin;
                var airBlock = GridEx.GetBlock(grid.grid, testPoint);
                var groundBlock = GridEx.GetBlock(grid.grid, testPoint - new Vector3Int(0, 1, 0));

                if (airBlock.type != BlockType.air && (groundBlock.type != BlockType.ground || groundBlock.type != BlockType.water))
                    return false;

                if (BuildingList.instance.GetBuildingAt(testPoint) != null)
                    return false;
            }
        }

        // invalid position is too close of player building or other spawner

        int nbBuilding = BuildingList.instance.GetBuildingNb();

        for(int i = 0; i < nbBuilding; i++)
        {
            var b = BuildingList.instance.GetBuildingFromIndex(i);
            Vector3 buildingPos = b.GetGroundCenter();

            float buildingDist = GridEx.GetDistance(grid.grid, new Vector2(buildingPos.x, buildingPos.z), new Vector2(realPosLoop.x, realPosLoop.z));

            if(b.GetTeam() == Team.Player)
            {
                if (buildingDist < m_asset.spawnerRadiusAroundPlayerBuildings)
                    return false;
            }
            else if(b.GetBuildingType() == BuildingType.EnnemySpawner)
            {
                if (buildingDist < m_asset.spawnerRadiusBetweenSpawner)
                    return false;
            }
        }

        pos = realPosLoop;
        return true;
    }
}
