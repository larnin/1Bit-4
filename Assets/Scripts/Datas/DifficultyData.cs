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
        return MathF.Pow(value * baseValue + addValue, powValue) * multiplierValue + finalAddValue;
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
    public float delayBetweenWavesMin;
    public float delayBetweenWavesMax;

    public DifficultyCurve difficultyToMaxEnnemies;

    public List<DifficultyOneEnnemyData> ennemies;
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
}

