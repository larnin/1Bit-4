using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TestEnnemiesData
{
    public EntityChoice entityType;
    public float weight;
}

[CreateAssetMenu(fileName = "TestEnnemiesMode", menuName = "Game/Gamemode/TestEnnemiesMode", order = 100)]
public class TestEnnemiesModeAsset : GamemodeAssetBase
{
    public string positionName;
    public float ennemiesPerSecond;
    public List<TestEnnemiesData> ennemies;
    public bool onlyOneEnnemy = false;
    public int burstCount = 0;
    public float burstDelay = 1;
    public bool burstOnKey = false;
    public bool spawnSpawnerOnKey = false;

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

    public override GamemodeBase MakeGamemode(GamemodeSystem owner)
    {
        TestEnnemiesMode mode = new TestEnnemiesMode(this, owner);

        return mode;
    }
}
