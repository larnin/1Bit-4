using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class DifficultyCurve
{
    public float baseValue;
    public float powValue;
    public float multiplierValue;
    public float addValue;
    public float finalAddValue;

    public float Get(float value)
    {
        float num = value * baseValue + addValue;
        if (num < 0)
            return finalAddValue;
        return MathF.Pow(num, powValue) * multiplierValue + finalAddValue;
    }
}

[Serializable]
public class DifficultyOneEnnemyData
{
    public GameObject prefab;
    public float difficultyMin;
    public float difficultyMax;
    public float difficultyCostMin;
    public float difficultyCostMax;
    public float weight;
}

[Serializable]
public class DifficultySpawnerData
{
    public GameObject prefab;
    public float delayBetweenWavesMin;
    public float delayBetweenWavesMax;
    public float distanceFromBuildingsMin;
    public float distanceFromBuildingsMax;
    public float distanceFromSpawnerMin;

    public float delayBaseBetweenEnnemies;
    public int delayReduceAfterNbEnnemies;
    public float delaySpeedMultiplayerPerEnnemies;

    public DifficultyCurve difficultyToMaxEnnemies;

    public List<DifficultyOneEnnemyData> ennemies;
}

[Serializable]
public class DifficultySizeMultiplier
{
    public WorldSize size;
    public float multiplier;
}

[Serializable]
public  class DifficultyData
{
    public DifficultyCurve difficultyPerMinute;
    public DifficultyCurve difficultyPerDistance;
    public DifficultyCurve difficultyPerKill;
    public DifficultyCurve difficultyPerSpawner;

    public DifficultyCurve difficultyToSpawnerNb;

    public DifficultySpawnerData spawnersData;

    public List<DifficultySizeMultiplier> difficultyMultipliers;
    public float GetDifficultyMultiplier(WorldSize size)
    {
        foreach(var d in difficultyMultipliers)
        {
            if (d.size == size)
                return d.multiplier;
        }

        return 1;
    }
}

