using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuestSubObjectiveStopQuest : QuestSubObjectiveBase
{
    string m_questName;

    public override bool IsCompleted()
    {
        return true;
    }

    public override void Start()
    {
        var questSystem = QuestSystem.instance;
        if (questSystem == null)
            return;

        questSystem.StopQuest(m_questName);
    }

    public override void Update(float deltaTime) { }
    
    public override void End() { }
}
