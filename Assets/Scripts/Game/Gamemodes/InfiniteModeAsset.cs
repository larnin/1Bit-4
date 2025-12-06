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
            return Mathf.Max(finalAddValue, 0);
        return Mathf.Max(MathF.Pow(num, powValue) * multiplierValue + finalAddValue, 0);
    }
}

[Serializable]
public class DifficultyOneEnnemyData
{
    public EntityChoice entityType;
    public float difficultyMin;
    public float difficultyMax;
    public float difficultyCostMin;
    public float difficultyCostMax;
    public float weight;
}

[Serializable]
public class DifficultySpawnerData
{
    public float displayHeight = 6;
    public float displayBeforeWave = 10;

    public float firstDelayAdd;
    public float delayBetweenWavesMin;
    public float delayBetweenWavesMax;
    public float delayDecreasePerLifePercentLoss;
    public float distanceFromBuildingsMin;
    public float distanceFromBuildingsMax;
    public float distanceFromSpawnerMin;

    public float delayBaseBetweenEnnemies;
    public int delayReduceAfterNbEnnemies;
    public float delaySpeedMultiplayerPerEnnemies;

    public float minSpawningIconDisplayTime;
    public float appearIconDisplayTime;

    public float timerDecreaseOnHit;

    public DifficultyCurve difficultyToMaxEnnemies;

    public List<DifficultyOneEnnemyData> ennemies;
}

[CreateAssetMenu(fileName = "InfiniteMode", menuName = "Game/Gamemode/InfiniteMode", order = 1)]
public class InfiniteModeAsset : GamemodeAssetBase
{
    public DifficultyCurve difficultyPerMinute;
    public DifficultyCurve difficultyPerDistance;
    public DifficultyCurve difficultyPerKill;
    public DifficultyCurve difficultyPerSpawner;

    public DifficultyCurve difficultyToSpawnerNb;

    public DifficultyCurve difficultyToLifeMultiplier;
    public DifficultyCurve difficultyToDamageMultiplier;

    public DifficultySpawnerData spawnersData;

    public override GamemodeBase GetGamemode(GameSystem owner)
    {
        throw new NotImplementedException();
    }
}
