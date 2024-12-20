using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public class SpawnerTest : MonoBehaviour
{
    [SerializeField] GameObject m_entityPrefab;
    [SerializeField] float m_spawnDelay = 1;
    [SerializeField] int m_maxCount = 1;

    float m_timer = 0;
    int m_count = 0;
    bool m_loaded = false;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnLoadEnd));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        m_timer = m_spawnDelay;
    }

    private void Update()
    {
        if (m_loaded)
        {
            m_timer -= Time.deltaTime;
            if (m_timer <= 0)
            {
                m_timer = m_spawnDelay;
                Spawn();
            }
        }
    }

    void Spawn()
    {
        m_count++;
        if (m_count > m_maxCount)
            return;

        GetGridEvent grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);

        if (grid.grid == null || m_entityPrefab == null)
            return;

        var size = GridEx.GetRealSize(grid.grid);

        var pos = Rand2D.UniformVector2CircleSurfaceDistribution(StaticRandomGenerator<MT19937>.Get());
        pos *= size / 6.1f;
        pos += new Vector2(size, size) / 2;

        var posInt = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        int height = GridEx.GetHeight(grid.grid, posInt);

        var obj = Instantiate(m_entityPrefab);
        obj.transform.parent = transform;
        obj.transform.position = new Vector3(posInt.x, height + 1, posInt.y);
    }

    void OnLoadEnd(GenerationFinishedEvent e)
    {
        m_loaded = true;
    }
}
