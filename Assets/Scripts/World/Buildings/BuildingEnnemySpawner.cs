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
    [SerializeField] float m_minSpawningIconDisplayTime;
    [SerializeField] float m_appearIconDisplayTime;
    [SerializeField] float m_timerDecreaseOnHit = 0.25f;

    enum State
    {
        Appear,
        Starting,
        Waiting,
        Spawning,
    }

    enum IconType
    {
        None,
        Appear,
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

    float m_iconDisplayDuration;
    IconType m_iconDisplayType = IconType.None;

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
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Add(new Event<LifeLossEvent>.LocalSubscriber(OnHit, gameObject));
        m_subscriberList.Subscribe();

        m_iconDisplayType = IconType.Appear;
        m_iconDisplayDuration = m_appearIconDisplayTime;
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
        if (Utility.IsFrozen(gameObject))
            return;

        if (Utility.IsDead(gameObject))
        {
            if (DisplayIconsV2.instance != null)
                DisplayIconsV2.instance.Unregister(gameObject);
            return;
        }

        if(EditorGridBehaviour.instance == null)
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

            if (DisplayIconsV2.instance != null)
            {
                if(m_iconDisplayType != IconType.None)
                {
                    m_iconDisplayDuration -= Mathf.Min(Time.deltaTime, 0.1f);

                    string iconName = "";

                    switch(m_iconDisplayType)
                    {
                        case IconType.Appear:
                            iconName = "Spawner";
                            break;
                        case IconType.Spawning:
                            iconName = "Spawning";
                            break;
                    }

                    DisplayIconsV2.instance.Register(gameObject, Global.instance.difficultyDatas.spawnersData.displayHeight, iconName, "", true, true);

                    if(m_iconDisplayDuration < 0)
                    {
                        DisplayIconsV2.instance.Unregister(gameObject);
                        m_iconDisplayType = IconType.None;
                    }
                }
            }
        }
    }

    void StartNextWait()
    {
        float min = Global.instance.difficultyDatas.spawnersData.delayBetweenWavesMin;
        float max = Global.instance.difficultyDatas.spawnersData.delayBetweenWavesMax;

        float time = Rand.UniformFloatDistribution(min, max, StaticRandomGenerator<MT19937>.Get());
        if (m_state == State.Starting)
            time += Global.instance.difficultyDatas.spawnersData.firstDelayAdd;

        m_timer = time;
        m_state = State.Waiting;
    }

    void StartNextWave(int forceCount = -1)
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

        int maxEnnemies = Mathf.CeilToInt(Global.instance.difficultyDatas.spawnersData.difficultyToMaxEnnemies.Get(difficulty));

        while (difficulty > 0)
        {
            var tabIndex = Rand.DiscreteDistribution(weights, rand);
            var index = allowedIndexs[tabIndex];
            m_entityIndexs.Add(index);

            if (forceCount > 0)
            {
                if (m_entityIndexs.Count >= forceCount)
                    break;
            }
            else
            {
                var e = Global.instance.difficultyDatas.spawnersData.ennemies[index];
                difficulty -= Rand.UniformFloatDistribution(e.difficultyCostMin, e.difficultyCostMax, rand);

                if (m_entityIndexs.Count >= maxEnnemies)
                    break;
            }
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
        if(DisplayIconsV2.instance != null && m_iconDisplayType == IconType.None)
        {
            string timer = Utility.FormateTime(m_timer, true);
            bool displayOutScreen = m_timer < Global.instance.difficultyDatas.spawnersData.displayBeforeWave;
            string iconName = "";
            if (displayOutScreen)
                iconName = "Warning";

            DisplayIconsV2.instance.Register(gameObject, Global.instance.difficultyDatas.spawnersData.displayHeight, iconName, timer, displayOutScreen);
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

        if (m_iconDisplayType != IconType.Spawning)
        {
            m_iconDisplayType = IconType.Spawning;
            m_iconDisplayDuration = m_minSpawningIconDisplayTime;
        }
        else m_iconDisplayDuration = Mathf.Max(m_iconDisplayDuration, 0.1f);
    }

    void SpawnOneEnnemie(int index)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        var e = Global.instance.difficultyDatas.spawnersData.ennemies[index];

        if (grid.grid == null || e.prefab == null)
            return;

        var pos = transform.position;

        var posInt = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        int height = GridEx.GetHeight(grid.grid, posInt);

        var obj = Instantiate(e.prefab);
        if(EntityList.instance != null)
            obj.transform.parent = EntityList.instance.transform;
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

    void OnHit(LifeLossEvent e)
    {
        if (m_state == State.Waiting)
            m_timer -= m_timerDecreaseOnHit;
    }

    string GetNextWaveTimer()
    {
        switch (m_state)
        {
            case State.Appear:
            case State.Starting:
                return "Waiting";
            case State.Waiting:
                return Utility.FormateTime(m_timer, true);
            case State.Spawning:
                return "Spawning";
        }

        return "";
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Time before next wave").SetTextFunc(GetNextWaveTimer);
    }

    protected override void LoadImpl(JsonObject obj)
    {
        var jsonTimer = obj.GetElement("timer");
        if (jsonTimer != null && jsonTimer.IsJsonNumber())
            m_timer = jsonTimer.Float();

        var jsonDeltaTime = obj.GetElement("deltaTime");
        if (jsonDeltaTime != null && jsonDeltaTime.IsJsonNumber())
            m_deltaTime = jsonDeltaTime.Float();

        var jsonState = obj.GetElement("state");
        if(jsonState != null && jsonState.IsJsonString())
        {
            if(!Enum.TryParse<State>(jsonState.String(), out m_state))
                m_state = State.Starting;
        }

        int nbEntities = 0;
        var jsonEntities = obj.GetElement("entities");
        if (jsonEntities != null && jsonEntities.IsJsonNumber())
            nbEntities = obj.Int();

        var jsonAppearStart = obj.GetElement("appearStart");
        if (jsonAppearStart != null && jsonAppearStart.IsJsonArray())
            m_appearStartPos = Json.ToVector3(jsonAppearStart.JsonArray());

        var jsonAppearEnd = obj.GetElement("appearEnd");
        if (jsonAppearEnd != null && jsonAppearEnd.IsJsonArray())
            m_appearEndPos = Json.ToVector3(jsonAppearEnd.JsonArray());

        var jsonAppearTimer = obj.GetElement("appearTimer");
        if (jsonAppearTimer != null && jsonAppearTimer.IsJsonNumber())
            m_appearTimer = jsonAppearTimer.Float();

        if (m_state == State.Spawning)
            StartNextWave(nbEntities);
    }

    protected override void SaveImpl(JsonObject obj)
    {
        obj.AddElement("timer", m_timer);
        obj.AddElement("deltaTime", m_deltaTime);
        obj.AddElement("state", m_state.ToString());
        obj.AddElement("entities", m_entityIndexs.Count() - m_currentIndex);

        obj.AddElement("appearStart", Json.FromVector3(m_appearStartPos));
        obj.AddElement("appearEnd", Json.FromVector3(m_appearEndPos));
        obj.AddElement("appearTimer", m_appearTimer);
    }
}

