using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSubObjectiveStartQuest : QuestSubObjectiveBase
{
    QuestScriptableObject m_quest;
    public QuestScriptableObject quest { get { return m_quest; } set { m_quest = value; } }

    public override bool IsCompleted()
    {
        return true;
    }

    public override void Start()
    {
        var questSystem = QuestSystem.instance;
        if (questSystem == null)
            return;

        if (m_quest == null)
            return;

        questSystem.StartQuest(m_quest.data, m_quest.name);
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
