using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;
using DG.Tweening;

public class TestEnnemiesMode : GamemodeBase
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

    TestEnnemiesModeAsset m_asset;

    List<BuildingEnnemySpawner> m_spawners = new List<BuildingEnnemySpawner>();
    float m_remainingTimer = 0;
    int m_nbSpawned = 0;
    float m_burstTimer = 0;


    LightState m_lightState = LightState.Idle;
    float m_lightTimer = 0;

    public TestEnnemiesMode(TestEnnemiesModeAsset asset, GamemodeSystem owner)
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
        if (GameInfos.instance.paused)
            return;

        if (!m_asset.spawnSpawnerOnKey || Input.GetKeyDown(KeyCode.K))
        {
            if (m_spawners.Count == 0)
            {
                SpawnSpawner();
            }
        }

        if (m_spawners.Count > 0)
        {
            if (m_asset.ennemiesPerSecond > 0)
            {
                m_remainingTimer += Time.deltaTime;
                int nbEnnemies = Mathf.FloorToInt(m_remainingTimer * m_asset.ennemiesPerSecond);
                m_remainingTimer -= nbEnnemies / m_asset.ennemiesPerSecond;

                for (int i = 0; i < nbEnnemies; i++)
                    SpawnOneEnnemy();
            }

            if (m_asset.burstCount > 0 && m_asset.burstDelay > 0 && !m_asset.burstOnKey)
            {
                m_burstTimer -= Time.deltaTime;
                if (m_burstTimer <= 0)
                {
                    m_burstTimer += m_asset.burstDelay;
                    for (int i = 0; i < m_asset.burstCount; i++)
                        SpawnOneEnnemy();

                }
            }

            if (m_asset.burstOnKey && Input.GetKeyDown(KeyCode.B))
            {
                for (int i = 0; i < m_asset.burstCount; i++)
                    SpawnOneEnnemy();
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
            TriggerMonolith(true);
        if (Input.GetKeyDown(KeyCode.L))
            TriggerMonolith(false);

        UpdateLight();
    }

    public override void Begin()
    {
        m_burstTimer = m_asset.burstDelay;
    }

    public override void End()
    {
        foreach (var s in m_spawners)
        {
            if(s != null)
                Event<KillEvent>.Broadcast(new KillEvent(), s.gameObject);
        }
        m_spawners.Clear();
    }

    void SpawnSpawner()
    {
        if (QuestElementList.instance == null)
            return;

        var objList = QuestElementList.instance.GetNamedObjectsByName(m_asset.positionName);
        if (objList.Count == 0)
            return;

        foreach (var obj in objList)
        {
            var pos = obj.transform.position;
            Vector3Int groundPos = new Vector3Int(Mathf.RoundToInt(pos.x), -1, Mathf.RoundToInt(pos.z));

            var grid = GridEx.GetCurrentGrid();
            if (grid == null)
                return;

            groundPos.y = GridEx.GetHeight(grid, new Vector2Int(groundPos.x, groundPos.z)) + 1;
            if (groundPos.y <= 0)
                return;
            pos.y = groundPos.y;

            var spawner = Global.instance.buildingDatas.GetBuilding(BuildingType.EnnemySpawner);
            if (spawner == null)
                return;

            var instance = GameObject.Instantiate(spawner.prefab);
            instance.transform.parent = BuildingList.instance.transform;
            instance.transform.position = pos;

            var building = instance.GetComponent<BuildingEnnemySpawner>();
            if (building == null)
                GameObject.Destroy(instance);
            else m_spawners.Add(building);
        }
    }

    void SpawnOneEnnemy()
    {
        if (m_asset.onlyOneEnnemy && m_nbSpawned > 0)
            return;

        int spawnerIndex = Rand.UniformIntDistribution(m_spawners.Count, StaticRandomGenerator<MT19937>.Get());
        var spawner = m_spawners[spawnerIndex];

        if (!spawner.HaveAppeared())
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

        var stratPos = spawner.GetGroundCenter();

        m_nbSpawned++;

        var obj = GameObject.Instantiate(prefab);
        if (EntityList.instance != null)
            obj.transform.parent = EntityList.instance.transform;
        obj.transform.position = stratPos;
    }

    void TriggerMonolith(bool startAngry)
    {
        if(BuildingList.instance != null)
        {
            var building = BuildingList.instance.GetFirstBuilding(BuildingType.Monolith);
            if(building != null)
            {
                var monolith = building as BuildingMonolith;
                if(monolith != null)
                {
                    if (startAngry)
                    {
                        StartLightState(LightState.Angry);
                        monolith.StartAngry();
                    }
                    else
                    {
                        StartLightState(LightState.WaveStart);
                        monolith.StartWave();
                    }
                }
            }
        }
    }

    void StartLightState(LightState newState)
    {
        if (m_lightState == newState)
            return;

        if (m_lightState == LightState.Idle && (newState == LightState.Angry || newState == LightState.AngryStart))
        {
            m_lightState = LightState.AngryStart;
            m_lightTimer = 0;
            return;
        }

        if (m_lightState == LightState.Angry)
        {
            if (newState == LightState.WaveStart)
            {
                m_lightState = LightState.WaveStart;
                m_lightTimer = 0;
                return;
            }

            if (newState == LightState.Idle)
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

        switch (m_lightState)
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
