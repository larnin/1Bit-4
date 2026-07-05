using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public class EntitySpawner : MonoBehaviour
{
    [Serializable]
    class EntityGroup
    {
        public EntityChoice entityType;
        public int count = 1;
        public float delayBeforeFirstGroup = 1;
        public float delayBetweenGroups = 1;
        public float delayBetweenEntities = 0.1f;
        public int groupCount = -1;
    }

    class EntityProcess
    {
        public float timer = 0;
        public bool spawning = false;
        public int groupCount = 0;
        public int spawnCount = 0;
    }

    [SerializeField] List<EntityGroup> m_spawnEntities;
    [SerializeField] float m_spawnMinRadius = 1;
    [SerializeField] float m_spawnMaxRadius = 3;
    [SerializeField] float m_spawnMinDistBetweenNpc = 1;
    [SerializeField] float m_spawnMinDistWithBuilding = 2;
    [SerializeField] float m_spawnMaxHeightDifference = 2;
    [SerializeField] bool m_spawnOnWater = false;

    List<EntityProcess> m_processEntities = new List<EntityProcess>();


    private void Start()
    {
        foreach(var g in m_spawnEntities)
        {
            EntityProcess e = new EntityProcess();
            e.spawning = false;
            e.groupCount = 0;
            e.spawnCount = 0;
            e.timer = g.delayBeforeFirstGroup;
            m_processEntities.Add(e);
        }
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (Utility.IsFrozen(gameObject))
            return;

        if (Utility.IsDead(gameObject))
            return;

        for(int i = 0; i < m_spawnEntities.Count; i++)
            UpdateGroup(m_spawnEntities[i], m_processEntities[i]);
    }

    void UpdateGroup(EntityGroup group, EntityProcess process)
    {
        if(process.spawning)
        {
            float time = process.timer + Time.deltaTime;
            int nbEntity = 0;
            if (group.delayBetweenEntities <= 0.001f)
                nbEntity = group.count;
            else nbEntity = Mathf.FloorToInt(time / group.delayBetweenEntities);

            if (nbEntity > group.count - process.spawnCount)
                nbEntity = group.count - process.spawnCount;

            for (int i = 0; i < nbEntity; i++)
                SpawnOneEntity(group.entityType.GetValue());

            process.spawnCount += nbEntity;
            process.timer = time - (nbEntity * group.delayBetweenEntities);

            if(process.spawnCount >= group.count)
            {
                process.timer = group.delayBetweenGroups;
                process.spawning = false;
            }
        }
        else
        {
            if (group.groupCount > 0 && group.groupCount <= process.groupCount)
                return;

            process.timer -= Time.deltaTime;

            if (process.timer <= 0)
            {
                process.timer = 0;
                process.spawnCount = 0;
                process.spawning = true;
                process.groupCount++;
            }
        }
    }

    void SpawnOneEntity(string entityType)
    {
        Vector3 pos;
        if (!GetValidSpawnPos(out pos))
            return;

        var prefab = Global.instance.editorDatas.GetEntityPrefab(entityType);
        if (prefab == null)
            return;

        var obj = GameObject.Instantiate(prefab);
        if (EntityList.instance != null)
            obj.transform.parent = EntityList.instance.transform;
        obj.transform.position = pos;
        
        foreach(var s in Enum.GetValues(typeof(StatType)))
        {
            var statType = (StatType)s;
            var stat = Event<GetStatEvent>.Broadcast(new GetStatEvent(statType, true));
            if (stat.value != 0)
                Event<AddStatEvent>.Broadcast(new AddStatEvent(statType, stat.value, "spawner"));
        }
    }

    bool GetValidSpawnPos(out Vector3 pos)
    {
        pos = Vector3.zero;

        var grid = GridEx.GetCurrentGrid();
        if (grid == null)
            return false;

        var rand = StaticRandomGenerator<MT19937>.Get();
        Vector3 center = transform.position;

        var testTeam = TeamEx.GetOppositeTeam(Event<GetTeamEvent>.Broadcast(new GetTeamEvent()).team);

        for(int i = 0; i < 5; i++)
        {
            Vector2 target = Rand2D.UniformVector2CircleSurfaceDistribution(rand);
            float radius = Rand.UniformFloatDistribution(m_spawnMinRadius, m_spawnMaxRadius, rand);
            target *= radius;
            target += new Vector2(center.x, center.z);

            Vector2Int targetI = new Vector2Int(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));

            int height = GridEx.GetHeight(grid, targetI);
            if (height < 0)
                continue;

            if (Mathf.Abs(height + 1 - center.y) > m_spawnMaxHeightDifference)
                continue;

            var block = GridEx.GetBlock(grid, new Vector3Int(targetI.x, height, targetI.y));
            if(block.type != BlockType.ground)
            {
                if (block.type != BlockType.water || !m_spawnOnWater)
                    continue; 
            }

            height++;

            Vector3 targetResult = new Vector3(targetI.x, height, targetI.y);

            var building = BuildingList.instance.GetNearestBuildingInRadius(targetResult, m_spawnMinDistWithBuilding, testTeam, AliveType.Alive);
            if (building != null)
                continue;

            var entity = EntityList.instance.GetNearestEntity(targetResult, m_spawnMinDistBetweenNpc, testTeam, AliveType.Alive);
            if (entity != null)
                continue;

            pos = targetResult;
            return true;
        }

        return false;
    }
}
