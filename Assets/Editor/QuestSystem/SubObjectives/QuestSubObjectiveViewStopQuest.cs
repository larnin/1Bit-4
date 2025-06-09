using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewStopQuest : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveStopQuest m_subObjective;

    public QuestSubObjectiveViewStopQuest(QuestSystemNodeObjective node, QuestSubObjectiveStopQuest subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        return QuestSystemEditorUtility.CreateTextField(m_subObjective.questName, "QuestName", OnQuestChange);
    }

    void OnQuestChange(ChangeEvent<string> quest)
    {
        m_subObjective.questName = quest.newValue;
    }
}
