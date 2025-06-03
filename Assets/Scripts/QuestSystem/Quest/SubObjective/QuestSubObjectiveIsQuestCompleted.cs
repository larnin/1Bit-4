using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuestSubObjectiveIsQuestCompleted : QuestSubObjectiveBase
{
    string m_questName;
    string m_objectiveName;

    public override bool IsCompleted()
    {
        var questSystem = QuestSystem.instance;
        if (questSystem == null)
            return false;

        if (m_objectiveName.Length == 0)
            return questSystem.IsQuestActive(m_questName);

        return questSystem.IsQuestObjectiveActive(m_questName, m_objectiveName);
    }

    public override void Start() { }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
