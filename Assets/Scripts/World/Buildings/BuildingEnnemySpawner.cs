using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class BuildingEnnemySpawner : BuildingBase
{
    [SerializeField] GameObject m_mesh;
    [SerializeField] float m_appearOffset;
    [SerializeField] float m_appearDuration;
    [SerializeField] Ease m_appearCurve;

    enum State
    {
        Appear,
        Starting,
        Waiting,
        Spawning,
    }

    float m_timer;
    float m_deltaTime;
    State m_state = State.Appear;
    List<int> m_entityIndexs = new List<int>();
    int m_currentIndex = 0;

    Vector3 m_appearEndPos;
    Vector3 m_appearStartPos;
    float m_appearTimer;
    float m_wantedLight;
    CustomLight m_light;

    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();

        m_appearEndPos = m_mesh.transform.localPosition;
        m_appearStartPos = m_appearEndPos - new Vector3(0, m_appearOffset, 0);

        m_light = GetComponentInChildren<CustomLight>();
        m_wantedLight = m_light.GetRadius();

        m_mesh.transform.localScale = new Vector3(0.99f, 0.99f, 0.99f);
        UpdateAppear();

        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Subscribe();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        m_subscriberList.Unsubscribe();
    }

    public override BuildingType GetBuildingType()
    {
        return BuildingType.EnnemySpawner;
    }

    protected override void OnUpdateAlways()
    {
        switch (m_state)
        {
            case State.Appear:
                if (UpdateAppear())
                    m_state = State.Starting;
                break;
            case State.Starting:
                StartNextWait();
                break;
            case State.Waiting:
                m_timer -= Time.deltaTime;
                ProcessWait();
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

    void ProcessWait()
    {
        if(DisplayIcons.instance != null)
        {
            string timer = Utility.FormateTime(m_timer, true);
            bool displayOutScreen = m_timer < Global.instance.difficultyDatas.spawnersData.displayBeforeWave;
            string iconName = "";
            if (displayOutScreen)
                iconName = "Warning";

            DisplayIcons.instance.Register(gameObject, Global.instance.difficultyDatas.spawnersData.displayHeight, iconName, timer, displayOutScreen);
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

        if(DisplayIcons.instance != null)
            DisplayIcons.instance.Register(gameObject, Global.instance.difficultyDatas.spawnersData.displayHeight, "Spawner", "", true, true);
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

    bool UpdateAppear()
    {
        bool ended = false;
        m_appearTimer += Time.deltaTime;

        float normTimer = m_appearTimer / m_appearDuration;

        if (normTimer > 1)
        {
            ended = true;
            normTimer = 1;
            m_mesh.transform.localScale = Vector3.one;
        }

        var pos = DOVirtual.EasedValue(m_appearStartPos, m_appearEndPos, normTimer, m_appearCurve);
        m_mesh.transform.localPosition = pos;

        float light = DOVirtual.EasedValue(0, m_wantedLight, normTimer, m_appearCurve);
        m_light.SetRadius(light);

        return ended;
    }

    void OnDeath(DeathEvent e)
    {
        Event<OnSpawnerDestroyEvent>.Broadcast(new OnSpawnerDestroyEvent());
    }
}

