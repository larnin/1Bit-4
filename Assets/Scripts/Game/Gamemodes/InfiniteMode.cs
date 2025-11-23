using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InfiniteMode : GamemodeBase
{
    class SpawnerInfo
    {
        public BuildingBase building;
    }

    InfiniteModeAsset m_asset;

    SubscriberList m_subscriberList;

    int m_nbKill = 0;
    int m_nbSpawnerDestroyed = 0;
    float m_maxDistance = 0;
    float m_maxDistanceTimer = 0;
    float m_time = 0;
    float m_maxDifficulty = 0;
    int m_nbSpawnerToSpawn = 0;

    List<SpawnerInfo> m_spawners = new List<SpawnerInfo>();

    public InfiniteMode(InfiniteModeAsset asset, GameSystem owner)
        : base(owner)
    {
        m_asset = asset;
    }

    public override void Begin()
    {
        if(m_subscriberList == null)
        {
            m_subscriberList = new SubscriberList();
            m_subscriberList.Add(new Event<OnKillEvent>.Subscriber(OnKill));
            m_subscriberList.Add(new Event<OnSpawnerDestroyEvent>.Subscriber(OnSpawnerDestroy));
        }

        m_subscriberList.Subscribe();
    }

    public override void Process()
    {
        if (GameInfos.instance.paused)
            return;

        m_time += Time.deltaTime;

        UpdateMaxDistance();
        UpdateSpawners();
    }

    public override void End()
    {
        m_subscriberList.Unsubscribe();
    }

    public override GamemodeStatus GetStatus()
    {
        return GamemodeStatus.Ongoing;
    }

    void OnKill(OnKillEvent e)
    {
        m_nbKill++;
    }

    void OnSpawnerDestroy(OnSpawnerDestroyEvent e)
    {
        m_nbSpawnerDestroyed++;
    }

    void UpdateMaxDistance()
    {
        m_maxDistanceTimer += Time.deltaTime;
        if (m_maxDistanceTimer > 0)
            return;

        m_maxDistanceTimer = 0.5f;

        if (ConnexionSystem.instance != null && BuildingList.instance != null)
        {
            var towers = BuildingList.instance.GetAllBuilding(BuildingType.Tower, Team.Player);

            if (towers.Count > 0)
            {
                var center = towers[0].GetGroundCenter();

                int nbConnexions = ConnexionSystem.instance.GetConnectedBuildingNb();
                for (int i = 0; i < nbConnexions; i++)
                {
                    var building = ConnexionSystem.instance.GetConnectedBuildingFromIndex(i);
                    var pos = building.GetGroundCenter();

                    var posFromCenter = pos - center;
                    float dist = posFromCenter.MagnitudeXZ();
                    if (dist > m_maxDistance)
                        m_maxDistance = dist;
                }
            }
        }
    }

    void UpdateSpawners()
    {
        float newDifficulty = GetDifficulty();
        if (newDifficulty > m_maxDifficulty)
        {
            int oldNbSpawner = Mathf.FloorToInt(m_asset.difficultyToSpawnerNb.Get(m_maxDifficulty));
            int newNbSpawner = Mathf.FloorToInt(m_asset.difficultyToSpawnerNb.Get(newDifficulty));

            if (newNbSpawner > oldNbSpawner)
                m_nbSpawnerToSpawn += newNbSpawner - oldNbSpawner;

            m_maxDifficulty = newDifficulty;
        }

        if (m_nbSpawnerToSpawn > 0)
            TrySpawnSpawner();
    }

    float GetDifficulty()
    {
        float difficultyPerMinute = m_asset.difficultyPerMinute.Get(m_time / 60);
        float difficultyPerDistance = m_asset.difficultyPerDistance.Get(m_maxDistance);
        float difficultyPerKill = m_asset.difficultyPerKill.Get(m_nbKill);
        float difficultyPerSpawner = m_asset.difficultyPerSpawner.Get(m_nbSpawnerDestroyed);

        float difficulty = (difficultyPerMinute + difficultyPerDistance + difficultyPerKill + difficultyPerSpawner);
        if (difficulty < 0)
            return 0;
        return difficulty;
    }

    void TrySpawnSpawner()
    {
        if (ConnexionSystem.instance == null)
            return;

        if (BuildingList.instance == null)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        int size = GridEx.GetRealSize(grid.grid);

        int nbBuilding = ConnexionSystem.instance.GetConnectedBuildingNb();

        var rand = StaticRandomGenerator<MT19937>.Get();

        var buildingIndex = Rand.UniformIntDistribution(nbBuilding, rand);
        var building = ConnexionSystem.instance.GetConnectedBuildingFromIndex(buildingIndex);

        var offsetDistance = Rand.UniformFloatDistribution(rand);
        offsetDistance = offsetDistance * m_asset.spawnersData.distanceFromBuildingsMin + (1 - offsetDistance) * m_asset.spawnersData.distanceFromBuildingsMax;
        var offset = Rand2D.UniformVector2CircleSurfaceDistribution(rand) * offsetDistance;

        var pos = building.GetGroundCenter() + new Vector3(offset.x, 0, offset.y);
        var posI = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

        if (posI.x < 1 || posI.z < 1 || posI.x >= size - 1 || posI.z >= size - 1)
            return;

        bool distOk = true;
        for (int j = 0; j < nbBuilding; j++)
        {
            Vector3 buildingPos = ConnexionSystem.instance.GetConnectedBuildingFromIndex(j).GetGroundCenter();

            float dist = (buildingPos - posI).SqrMagnitudeXZ();
            if (dist < m_asset.spawnersData.distanceFromBuildingsMin * m_asset.spawnersData.distanceFromBuildingsMin)
            {
                distOk = false;
                break;
            }
        }

        if (!distOk)
            return;

        int height = GridEx.GetHeight(grid.grid, new Vector2Int(posI.x, posI.z));
        if (height < 0)
            return;

        bool posOk = true;
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                int tempHeight = GridEx.GetHeight(grid.grid, new Vector2Int(posI.x + x, posI.z + z));
                if (tempHeight != height)
                {
                    posOk = false;
                    break;
                }
            }
            if (!posOk)
                break;
        }

        if (!posOk)
            return;

        Vector3 spawnPos = new Vector3(posI.x, height + 1, posI.z);
        var testBuilding = BuildingList.instance.GetNearestBuildingInRadius(spawnPos, m_asset.spawnersData.distanceFromSpawnerMin);

        posOk = testBuilding != null;

        if (posOk)
        {
            var spawner = Global.instance.buildingDatas.GetBuilding(BuildingType.EnnemySpawner);
            if (spawner != null)
            {
                var obj = GameObject.Instantiate(spawner.prefab);
                obj.transform.parent = BuildingList.instance.transform;
                obj.transform.position = spawnPos;

                SpawnerInfo spawnerInfo = new SpawnerInfo();
                spawnerInfo.building = obj.GetComponent<BuildingBase>();
                if (spawnerInfo.building == null)
                    Debug.LogError("No building in the prefab " + spawner.prefab.name);

                m_spawners.Add(spawnerInfo);
            }

            m_nbSpawnerToSpawn--;

            return;
        }
    }
}

