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
    public float scoreIncreasePerAngryMonolith = 0;
    public float spawnerSpawnDuration = 5;

    public List<MonolithModeEnnemy> ennemies = new List<MonolithModeEnnemy>();

    public override GamemodeBase MakeGamemode(GamemodeSystem owner)
    {
        MonolithMode mode = new MonolithMode(this, owner);

        return mode;
    }
}

