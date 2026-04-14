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

    public override GamemodeBase MakeGamemode(GamemodeSystem owner)
    {
        TestEnnemiesMode mode = new TestEnnemiesMode(this, owner);

        return mode;
    }
}
