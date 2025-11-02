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
public class LevelInfo
{
    public string name;
    public string description;
    public Sprite icon;
    public LevelUnlockCondition unlockCondition;
    public JsonScriptableObject level;
    public QuestScriptableObject quest;
}

[Serializable]
public class LevelsData
{
    public List<LevelInfo> Levels = new List<LevelInfo>();

    public LevelInfo InfiniteMode = new LevelInfo();

    public LevelInfo GetLevelInfo(int index, bool infiniteMode = false)
    {
        if (infiniteMode)
            return InfiniteMode;
        if (index >= 0 && index < Levels.Count)
            return Levels[index];

        return null;
    }
}
