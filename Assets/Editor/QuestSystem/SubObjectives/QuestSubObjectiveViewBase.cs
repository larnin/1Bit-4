using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public abstract class QuestSubObjectiveViewBase
{
    protected QuestSystemNodeObjective m_node;
    protected QuestSubObjectiveBase m_subObjective;

    public QuestSubObjectiveViewBase(QuestSystemNodeObjective node, QuestSubObjectiveBase subObjective)
    {
        m_node = node;
        m_subObjective = subObjective;
    }

    public QuestSubObjectiveBase GetSubObjective()
    {
        return m_subObjective;
    }

    public static QuestSubObjectiveViewBase Create(QuestSystemNodeObjective node, QuestSubObjectiveBase subObjective)
    {
        if (subObjective is QuestSubObjectiveTimer)
            return new QuestSubObjectiveViewTimer(node, subObjective as QuestSubObjectiveTimer);
        if (subObjective is QuestSubObjectiveIsQuestCompleted)
            return new QuestSubObjectiveViewIsQuestCompleted(node, subObjective as QuestSubObjectiveIsQuestCompleted);
        if (subObjective is QuestSubObjectiveStopQuest)
            return new QuestSubObjectiveViewStopQuest(node, subObjective as QuestSubObjectiveStopQuest);
        if (subObjective is QuestSubObjectiveStartQuest)
            return new QuestSubObjectiveViewStartQuest(node, subObjective as QuestSubObjectiveStartQuest);
        if (subObjective is QuestSubObjectiveGroup)
            return new QuestSubObjectiveViewGroup(node, subObjective as QuestSubObjectiveGroup);
        if (subObjective is QuestSubObjectiveFailAfterTimer)
            return new QuestSubObjectiveViewFailAfterTimer(node, subObjective as QuestSubObjectiveFailAfterTimer);

        return new QuestSubObjectiveViewNotImplemented(node, subObjective);
    }

    public VisualElement GetElement()
    {
        VisualElement element = GetElementInternal();
        if(!m_subObjective.CanFail())
            return element;

        VisualElement container = new VisualElement();
        container.Add(element);
        container.Add(QuestSystemEditorUtility.CreateTextField(m_subObjective.failNodeName, "Fail Output", OnFailNodeChange));

        return container;
    }

    protected abstract VisualElement GetElementInternal();

    void OnFailNodeChange(ChangeEvent<string> newName)
    {
        m_subObjective.failNodeName = newName.newValue;
        m_node.OnOutputChange();
    }
}
