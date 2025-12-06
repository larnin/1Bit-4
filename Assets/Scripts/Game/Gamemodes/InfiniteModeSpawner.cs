using NRand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InfiniteModeSpawner
{
    enum State
    {
        Appear,
        Spawning,
        Waiting
    }

    enum IconType
    {
        None,
        Appear,
        Spawning,
    }

    InfiniteMode m_mode;
    BuildingBase m_building;

    State m_state = State.Appear;
    float m_timer = 0;
    float m_iconDisplayDuration;
    IconType m_iconDisplayType = IconType.None;

    List<int> m_entityIndexs = new List<int>();
    int m_currentIndex = 0;
    float m_spawnDeltaTime = 0;

    public InfiniteModeSpawner(InfiniteMode mode, BuildingBase building)
    {
        m_mode = mode;
        m_building = building;

        m_iconDisplayType = IconType.Appear;
        m_iconDisplayDuration = m_mode.GetInfiniteAsset().spawnersData.appearIconDisplayTime;
    }

    public BuildingBase GetBuilding()
    {
        return m_building;
    }


    public void Update(float deltaTime)
    {
        if (Utility.IsFrozen(m_building.gameObject))
            return;

        if (Utility.IsDead(m_building.gameObject))
        {
            if (DisplayIconsV2.instance != null)
                DisplayIconsV2.instance.Unregister(m_building.gameObject);
            return;
        }

        switch(m_state)
        {
            case State.Appear:
                UpdateAppear(deltaTime);
                break;
            case State.Spawning:
                UpdateSpawning(deltaTime);
                break;
            case State.Waiting:
                UpdateWaiting(deltaTime);
                break;
        }

        UpdateIcon(deltaTime);
    }

    public void OnHit(float lifePercent)
    {
        if (m_state == State.Waiting)
            m_timer -= m_mode.GetInfiniteAsset().spawnersData.delayDecreasePerLifePercentLoss * lifePercent;
    }

    void UpdateAppear(float deltaTime)
    {
        bool startNext = false;
        BuildingEnnemySpawner Spawner = m_building as BuildingEnnemySpawner;
        if (Spawner == null)
            startNext = true;
        else if (Spawner.HaveAppeared())
            startNext = true;

        if(startNext)
            StartNextWait();
    }

    void StartNextWave(int forceCount = -1)
    {
        InfiniteModeAsset asset = m_mode.GetInfiniteAsset();

        float difficulty = m_mode.GetDifficulty();

        List<int> allowedIndexs = new List<int>();
        int nbEnnemies = asset.spawnersData.ennemies.Count;
        for (int i = 0; i < nbEnnemies; i++)
        {
            var e = asset.spawnersData.ennemies[i];

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
            weights.Add(asset.spawnersData.ennemies[index].weight);

        m_entityIndexs.Clear();
        m_currentIndex = 0;
        m_timer = 0;
        m_state = State.Spawning;

        var rand = StaticRandomGenerator<MT19937>.Get();

        int maxEnnemies = Mathf.CeilToInt(asset.spawnersData.difficultyToMaxEnnemies.Get(difficulty));

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
                var e = asset.spawnersData.ennemies[index];
                difficulty -= Rand.UniformFloatDistribution(e.difficultyCostMin, e.difficultyCostMax, rand);

                if (m_entityIndexs.Count >= maxEnnemies)
                    break;
            }
        }

        m_entityIndexs.Shuffle(rand);

        m_spawnDeltaTime = asset.spawnersData.delayBaseBetweenEnnemies;
        int nbMore = m_entityIndexs.Count - asset.spawnersData.delayReduceAfterNbEnnemies;
        if (nbMore > 0)
        {
            float multiplier = 1 + nbMore * asset.spawnersData.delaySpeedMultiplayerPerEnnemies;
            m_spawnDeltaTime /= multiplier;
        }
    }

    void UpdateSpawning(float deltaTime)
    {
        m_timer += deltaTime;
        while (m_timer >= m_spawnDeltaTime)
        {
            m_timer -= m_spawnDeltaTime;

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
            m_iconDisplayDuration = m_mode.GetInfiniteAsset().spawnersData.minSpawningIconDisplayTime;
        }
        else m_iconDisplayDuration = Mathf.Max(m_iconDisplayDuration, 0.1f);
    }

    void SpawnOneEnnemie(int index)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());

        var e = m_mode.GetInfiniteAsset().spawnersData.ennemies[index];

        if (grid.grid == null)//|| e.prefab == null)
            return;

        var pos = m_building.transform.position;

        var posInt = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        int height = GridEx.GetHeight(grid.grid, posInt);

        var prefab = Global.instance.editorDatas.GetEntityPrefab(e.entityType.GetValue());

        if (prefab != null)
        {
            var obj = GameObject.Instantiate(prefab);
            if (EntityList.instance != null)
                obj.transform.parent = EntityList.instance.transform;
            obj.transform.position = new Vector3(posInt.x, height + 1, posInt.y);
        }
    }

    void StartNextWait()
    {
        float min = m_mode.GetInfiniteAsset().spawnersData.delayBetweenWavesMin;
        float max = m_mode.GetInfiniteAsset().spawnersData.delayBetweenWavesMax;

        float time = Rand.UniformFloatDistribution(min, max, StaticRandomGenerator<MT19937>.Get());
        if (m_state == State.Appear)
            time += m_mode.GetInfiniteAsset().spawnersData.firstDelayAdd;

        m_timer = time;
        m_state = State.Waiting;
    }

    void UpdateWaiting(float deltaTime)
    {
        if (DisplayIconsV2.instance != null && m_iconDisplayType == IconType.None)
        {
            string timer = Utility.FormateTime(m_timer, true);
            bool displayOutScreen = m_timer < m_mode.GetInfiniteAsset().spawnersData.displayBeforeWave;
            string iconName = "";
            if (displayOutScreen)
                iconName = "Warning";

            DisplayIconsV2.instance.Register(m_building.gameObject, m_mode.GetInfiniteAsset().spawnersData.displayHeight, iconName, timer, displayOutScreen);
        }

        m_timer -= deltaTime;
        if (m_timer < 0)
            StartNextWave();
    }

    void UpdateIcon(float deltaTime)
    {
        if (DisplayIconsV2.instance == null)
            return;

        {
            if (m_iconDisplayType != IconType.None)
            {
                m_iconDisplayDuration -= Mathf.Min(deltaTime, 0.1f);

                string iconName = "";

                switch (m_iconDisplayType)
                {
                    case IconType.Appear:
                        iconName = "Spawner";
                        break;
                    case IconType.Spawning:
                        iconName = "Spawning";
                        break;
                }

                DisplayIconsV2.instance.Register(m_building.gameObject, m_mode.GetInfiniteAsset().spawnersData.displayHeight, iconName, "", true, true);

                if (m_iconDisplayDuration < 0)
                {
                    DisplayIconsV2.instance.Unregister(m_building.gameObject);
                    m_iconDisplayType = IconType.None;
                }
            }
        }
    }

    string GetNextWaveTimer()
    {
        switch (m_state)
        {
            case State.Appear:
            case State.Waiting:
                return "Waiting " + Utility.FormateTime(m_timer, true);
            case State.Spawning:
                return "Spawning";
        }

        return "";
    }

    public void DisplayInfos(UIElementContainer container)
    {
        UIElementData.Create<UIElementLabelAndText>(container).SetLabel("Time before next wave").SetTextFunc(GetNextWaveTimer);
    }
}
