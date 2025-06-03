using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSubObjectiveStartQuest : QuestSubObjectiveBase
{
    ScriptableObject m_quest;

    public override bool IsCompleted()
    {
        return true;
    }

    public override void Start()
    {
        var questSystem = QuestSystem.instance;
        if (questSystem == null)
            return;

        var quest = m_quest as QuestScriptableObject;
        if (quest == null)
            return;

        questSystem.StartQuest(quest.data);
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
