using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum LevelUnlockCondition
{
    WhenPreviousCompleted,
    UnlockedByDefault,
}

[Serializable]
public class InitialResource
{
    public ResourceType type;
    public float count;
}

[Serializable]
public class LevelInfo
{
    public string name;
    public string description;
    public Sprite icon;
    public LevelUnlockCondition unlockCondition;
    public JsonScriptableObject level;
    public QuestScriptableObject quest;
    public List<InitialResource> initialResources = new List<InitialResource>();
}

[Serializable]
public class InfiniteLevelInfo
{
    public int size = 16;
    public int height = 4;

    public bool loopX = false;
    public bool loopZ = false;

    public string presetName;
}

[Serializable]
public class LevelsData
{
    public QuestScriptableObject globalQuest;

    public List<LevelInfo> Levels = new List<LevelInfo>();

    public LevelInfo InfiniteMode = new LevelInfo();
    public InfiniteLevelInfo InfiniteLevel = new InfiniteLevelInfo();

    public LevelInfo GetLevelInfo(int index, bool infiniteMode = false)
    {
        if (infiniteMode)
            return InfiniteMode;
        if (index >= 0 && index < Levels.Count)
            return Levels[index];

        return null;
    }
}
