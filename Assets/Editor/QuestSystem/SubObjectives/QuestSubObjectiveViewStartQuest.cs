using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewStartQuest : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveStartQuest m_subObjective;

    public QuestSubObjectiveViewStartQuest(QuestSystemNodeObjective node, QuestSubObjectiveStartQuest subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        return QuestSystemEditorUtility.CreateObjectField("Quest", typeof(QuestScriptableObject), false, m_subObjective.quest, OnQuestChange);
    }

    void OnQuestChange(ChangeEvent<UnityEngine.Object> quest)
    {
        var scr = quest.newValue as QuestScriptableObject;
        if (scr == null)
            return;

        m_subObjective.quest = scr;
    }
}
