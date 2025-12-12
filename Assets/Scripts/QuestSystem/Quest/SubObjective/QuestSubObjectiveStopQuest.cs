using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestSubObjectiveStopQuest : QuestSubObjectiveBase
{
    [SerializeField]
    string m_questName;
    public string questName { get { return m_questName; } set { m_questName = value; } }

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
