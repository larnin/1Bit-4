using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum QuestObjectiveCompletionType
{
    NotStarted,
    Ongoing,
    Completed
}

public class QuestSubObjectiveIsQuestCompleted : QuestSubObjectiveBase
{
    string m_questName;
    public string questName { get { return m_questName; } set { m_questName = value; } }

    string m_objectiveName;
    public string objectiveName { get { return m_objectiveName; } set { m_objectiveName = value; } }

    QuestObjectiveCompletionType m_completionType = QuestObjectiveCompletionType.Completed;
    public QuestObjectiveCompletionType completionType { get { return m_completionType; } set { m_completionType = value; } }

    public override bool IsCompleted()
    {
        var questSystem = QuestSystem.instance;
        if (questSystem == null)
            return false;

        if (m_objectiveName.Length == 0)
            return questSystem.GetQuestStatus(m_questName) == m_completionType;

        return questSystem.GetQuestObjectiveStatus(m_questName, m_objectiveName) == m_completionType;
    }

    public override void Start() { }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
