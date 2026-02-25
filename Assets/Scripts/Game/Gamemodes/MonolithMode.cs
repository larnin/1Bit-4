using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class MonolithMode : GamemodeBase
{
    enum LightState
    {
        Idle,
        AngryStart,
        Angry,
        AngryEnd,
        WaveStart,
        WaveTop,
        WaveEnd,
    }

    class BuildingStatus
    {
        public BuildingMonolith building;
        public float timer = 0;
        public List<MonolithModeSpawner> spawners = new List<MonolithModeSpawner>();
        public float nullifyPower = 0;
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

    LightState m_lightState = LightState.Idle;
    float m_lightTimer = 0;

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
            m_subscriberList.Add(new Event<OnBuildingDamagedEvent>.Subscriber(OnBuildingDamaged));
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
        bool oneAngry = false;
        foreach(var b in m_aliveBuildings)
            oneAngry |= UpdateAngryBuilding(b, deltaTime);

        if (oneAngry)
            m_currentScore += m_asset.scorePerMinute * Time.deltaTime / 60;

        StartLightState(oneAngry ? LightState.Angry : LightState.Idle);

        UpdateNewSpawners();

        UpdateLight();
    }

    public override void End() 
    {
        m_subscriberList.Unsubscribe();
    }

    void OnBuildingDamaged(OnBuildingDamagedEvent e)
    {

    }

    void OnBuildingDestroyed(OnBuildingDestroyEvent e)
    {

    }

    public void TriggerMonolith(BuildingMonolith building)
    {
        var data = m_aliveBuildings.Find(x => { return x.building = building; });
        if (data == null)
            return;

        building.StartAngry();
    }

    bool UpdateAngryBuilding(BuildingStatus building, float deltaTime)
    {
        if (building.building == null)
            return false;

        var state = building.building.GetState();
        bool returnValue = state == BuildingMonolith.State.AngryLoop || state == BuildingMonolith.State.AngryStart || state == BuildingMonolith.State.Wave;

        if (state == BuildingMonolith.State.AngryLoop || state == BuildingMonolith.State.Wave)
        {
            var nullifier = building.building.GetNullifier();
            if (nullifier != null)
                building.nullifyPower += nullifier.GetEfficiency() * deltaTime;

            if (building.nullifyPower >= m_asset.nullifyDuration)
            {
                BuildingNullified(building);
                return returnValue;
            }
        }

        if (state != BuildingMonolith.State.AngryLoop)
            return returnValue;

        building.timer += deltaTime;

        if(building.timer >= m_asset.delayBetweenWave)
        {
            StartWaveFromBuilding(building);
            building.timer = 0;
            building.building.StartWave();
            StartLightState(LightState.WaveStart);
        }

        return returnValue;
    }

    void BuildingNullified(BuildingStatus building)
    {
        if (building.building == null)
            return;

        building.building.StartNullified();
    }

    void UpdateAliveMonoliths()
    {
        float deltaTime = Time.deltaTime;

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

            foreach (var s in m.spawners)
                s.Update(deltaTime);
        }

        foreach(var r in toRemove)
        {
            m_aliveBuildings.Remove(r);

            foreach(var s in r.spawners)
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
        foreach(var spawner in building.spawners)
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
            count += monolith.spawners.Count;

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

            data.source.spawners.Add(spawnerInfos);
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
        
        Vector3Int buildingMin = -spawner.size / 2;
        buildingMin.y = 0;

        for(int i = 0; i < spawner.size.x; i++)
        {
            for(int j = 0; j < spawner.size.z; j++)
            {
                Vector3Int testPoint = realPosLoop + new Vector3Int(i, 0, j) + buildingMin;
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

    void StartLightState(LightState newState)
    {
        if (m_lightState == newState)
            return;

        if(m_lightState == LightState.Idle && (newState == LightState.Angry || newState == LightState.AngryStart))
        {
            m_lightState = LightState.AngryStart;
            m_lightTimer = 0;
            return;
        }

        if(m_lightState == LightState.Angry)
        {
            if(newState == LightState.WaveStart)
            {
                m_lightState = LightState.WaveStart;
                m_lightTimer = 0;
                return;
            }

            if(newState == LightState.Idle)
            {
                m_lightState = LightState.AngryEnd;
                m_lightTimer = 0;
                return;
            }
        }
    }

    void UpdateLight()
    {
        if (CustomLightsManager.instance == null)
            return;

        var light = CustomLightsManager.instance.GetDefaultLightParams();

        m_lightTimer += Time.deltaTime;

        switch(m_lightState)
        {
            case LightState.Idle:
            default:
                //nothing
                break;
            case LightState.AngryStart:
            case LightState.AngryEnd:
                {
                    float percent = m_lightTimer / m_asset.angryLightTransitionTime;
                    if (m_lightState == LightState.AngryEnd)
                        percent = 1 - percent;
                    percent = DOVirtual.EasedValue(0, 1, percent, m_asset.angryLightTransitionCurve);
                    light.noiseAmplitude += percent * m_asset.angryLightNoiseAmplitude;
                    light.noiseSpeed += percent * m_asset.angryLightNoiseSpeedOffset;
                    light.lightBaseRange += percent * m_asset.angryLightBaseRange;

                    if (m_lightTimer >= m_asset.angryLightTransitionTime)
                    {
                        m_lightTimer = 0;
                        m_lightState = m_lightState == LightState.AngryStart ? LightState.Angry : LightState.Idle;
                    }

                    break;
                }
            case LightState.Angry:
                    light.noiseAmplitude += m_asset.angryLightNoiseAmplitude;
                    light.noiseSpeed += m_asset.angryLightNoiseSpeedOffset; 
                light.lightBaseRange += m_asset.angryLightBaseRange;
                break;
            case LightState.WaveStart:
            case LightState.WaveEnd:
                {
                    float transitionTime = m_lightState == LightState.WaveStart ? m_asset.waveLightTransitionTimeIn : m_asset.waveLightTransitionTimeOut;
                    float percent = m_lightTimer / transitionTime;
                    if (m_lightState == LightState.WaveEnd)
                        percent = 1 - percent;
                    percent = DOVirtual.EasedValue(0, 1, percent, m_asset.waveLightTransitionCurve);
                    light.noiseAmplitude += (1 - percent) * m_asset.angryLightNoiseAmplitude + percent * m_asset.waveLightNoiseAmplitude;
                    light.noiseSpeed += (1 - percent) * m_asset.angryLightNoiseSpeedOffset + percent * m_asset.waveLightNoiseSpeedOffset;
                    light.increaseRadius += percent * m_asset.waveLightIncreaseRadius;
                    light.lightBaseRange += (1 - percent) * m_asset.angryLightBaseRange;

                    if (m_lightTimer >= transitionTime)
                    {
                        m_lightTimer = 0;
                        m_lightState = m_lightState == LightState.WaveEnd ? LightState.Angry : LightState.WaveTop;
                    }

                    break;
                }
            case LightState.WaveTop:
                light.noiseAmplitude += m_asset.waveLightNoiseAmplitude;
                light.noiseSpeed += m_asset.waveLightNoiseSpeedOffset;
                light.increaseRadius += m_asset.waveLightIncreaseRadius;

                if (m_lightTimer >= m_asset.waveLightTopTime)
                {
                    m_lightTimer = 0;
                    m_lightState = LightState.WaveEnd;
                }
                break;
        }

        CustomLightsManager.instance.SetCurrentLightParams(light);
    }
}
