using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

[Serializable]
public class TestEnnemiesData
{
    public EntityChoice entityType;
    public float weight;
}

[CreateAssetMenu(fileName = "TestEnnemiesMode", menuName = "Game/Gamemode/TestEnnemiesMode", order = 100)]
public class TestEnnemiesGamemodeAsset : GamemodeAssetBase
{
    public string positionName;
    public float ennemiesPerSecond;
    public List<TestEnnemiesData> ennemies;

    public override GamemodeBase MakeGamemode(GamemodeSystem owner)
    {
        TestEnnemiesGamemode mode = new TestEnnemiesGamemode(this, owner);

        return mode;
    }
}

public class TestEnnemiesGamemode : GamemodeBase
{
    TestEnnemiesGamemodeAsset m_asset;

    BuildingEnnemySpawner m_spawner = null;
    float m_remainingTimer = 0;

    public TestEnnemiesGamemode(TestEnnemiesGamemodeAsset asset, GamemodeSystem owner)
        : base(owner)
    {
        m_asset = asset;
    }

    public override GamemodeAssetBase GetAsset()
    {
        return m_asset;
    }

    public override GamemodeStatus GetStatus()
    {
        return GamemodeStatus.Ongoing;
    }

    public override void Process()
    {
        if(m_spawner == null)
        {
            SpawnSpawner();
            if (m_spawner == null)
                return;
        }

        m_remainingTimer += Time.deltaTime;
        int nbEnnemies = Mathf.FloorToInt(m_remainingTimer * m_asset.ennemiesPerSecond);
        m_remainingTimer -= nbEnnemies / m_asset.ennemiesPerSecond;

        for(int i = 0; i < nbEnnemies; i++)
            SpawnOneEnnemy();
    }

    public override void End()
    {
        Event<HitEvent>.Broadcast(new HitEvent(new Hit(10000000)), m_spawner.gameObject);
    }

    void SpawnSpawner()
    {
        if (QuestElementList.instance == null)
            return;

        var obj = QuestElementList.instance.GetFirstNamedObjectByName(m_asset.positionName);
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

        var spawner = Global.instance.buildingDatas.GetBuilding(BuildingType.EnnemySpawner);
        if (spawner == null)
            return;

        var instance = GameObject.Instantiate(spawner.prefab);
        instance.transform.parent = BuildingList.instance.transform;
        instance.transform.position = pos;

        m_spawner = instance.GetComponent<BuildingEnnemySpawner>();
        if (m_spawner == null)
            GameObject.Destroy(instance);
    }

    void SpawnOneEnnemy()
    {
        if (m_spawner == null)
            return;

        List<float> weights = new List<float>();
        foreach (var e in m_asset.ennemies)
            weights.Add(e.weight);

        if (weights.Count == 0)
            return;

        int index = Rand.DiscreteDistribution(weights, StaticRandomGenerator<MT19937>.Get());

        var prefab = Global.instance.editorDatas.GetEntityPrefab(m_asset.ennemies[index].entityType.GetValue());
        if (prefab == null)
            return;

        var stratPos = m_spawner.GetGroundCenter();

        var obj = GameObject.Instantiate(prefab);
        if (EntityList.instance != null)
            obj.transform.parent = EntityList.instance.transform;
        obj.transform.position = stratPos;
    }
}
