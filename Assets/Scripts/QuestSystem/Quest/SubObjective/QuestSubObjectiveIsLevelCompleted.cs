using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuestSubObjectiveIsLevelCompleted : QuestSubObjectiveBase
{
    [SerializeField]
    string m_levelName;
    public string levelName { get { return m_levelName; } set { m_levelName = value; } }

    [SerializeField]
    QuestObjectiveCompletionType m_completionType = QuestObjectiveCompletionType.Completed;
    public QuestObjectiveCompletionType completionType { get { return m_completionType; } set { m_completionType = value; } }

    public override bool IsCompleted()
    {
        bool completed = GameInfos.instance.persistant.IsLevelCompleted(m_levelName);

        if (m_completionType == QuestObjectiveCompletionType.Completed && completed)
            return true;

        if (GameInfos.instance.gameParams.level != null)
        {
            bool onGoing = GameInfos.instance.gameParams.level.name == m_levelName;
            if (m_completionType == QuestObjectiveCompletionType.Ongoing && onGoing && !completed)
                return true;

            if (m_completionType == QuestObjectiveCompletionType.NotStarted && !completed && !onGoing)
                return true;
        }
        else if (m_completionType == QuestObjectiveCompletionType.NotStarted && !completed)
            return true;

        return false;
    }

    public override void Start() { }

    public override void Update(float deltaTime) { }

    public override void End() { }

}
