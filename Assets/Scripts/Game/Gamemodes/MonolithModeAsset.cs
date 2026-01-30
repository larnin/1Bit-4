using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class MonolithModeEnnemy
{
    public EntityChoice entityType;
    public float scoreMin = -1;
    public float scoreMax = -1;
    public float cost = 1;
    public float weight = 1;
}

[CreateAssetMenu(fileName = "MonolithMode", menuName = "Game/Gamemode/MonolithMode", order = 1)]
public class MonolithModeAsset : GamemodeAssetBase
{
    public float delayBetweenWave = 30;
    public float spawnerRadiusAroundMonolith = 50;
    public float spawnerRadiusBetweenSpawner = 5;
    public float spawnerRadiusAroundPlayerBuildings = 5;
    public int spawnerMaxNb = 20;
    public int spawnerIncreaseMaxPerAngryMonolith = 5;
    public int spawnerPerWave = 5;
    public float initialScore = 10;
    public float scorePerMinute = 1;
    public float scorePerLostPercent = 0;
    public float nullifyDuration = 150;
    public float spawnerSpawnDuration = 5;

    public float angryLightNoiseSpeedOffset = 1;
    public float angryLightNoiseAmplitude = 1;
    public float angryLightBaseRange = 1;
    public float angryLightTransitionTime = 1;
    public Ease angryLightTransitionCurve = Ease.Linear;

    public float waveLightIncreaseRadius = -1;
    public float waveLightNoiseSpeedOffset = 1;
    public float waveLightNoiseAmplitude = 1;
    public float waveLightTransitionTimeIn = 0.5f;
    public float waveLightTransitionTimeOut = 0.5f;
    public float waveLightTopTime = 0.2f;
    public Ease waveLightTransitionCurve = Ease.Linear;



    public List<MonolithModeEnnemy> ennemies = new List<MonolithModeEnnemy>();

    public override GamemodeBase MakeGamemode(GamemodeSystem owner)
    {
        MonolithMode mode = new MonolithMode(this, owner);

        return mode;
    }
}

