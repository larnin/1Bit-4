using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewIsQuestCompleted : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveIsQuestCompleted m_subObjective;

    public QuestSubObjectiveViewIsQuestCompleted(QuestSystemNodeObjective node, QuestSubObjectiveIsQuestCompleted subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    public override VisualElement GetElement()
    {
        var element = new VisualElement();
        element.Add(QuestSystemEditorUtility.CreateTextField(m_subObjective.questName, "Quest Name", OnQuestChange));
        var objectiveName = QuestSystemEditorUtility.CreateTextField(m_subObjective.objectiveName, "ObjectiveName", OnObjectiveChange);
        objectiveName.tooltip = "If empty, check the completion of the whole quest";
        element.Add(objectiveName);

        var completion = new EnumField("Completion Type", m_subObjective.completionType);
        completion.RegisterValueChangedCallback(OnCompletionChange);
        element.Add(completion);

        return element;
    }

    void OnQuestChange(ChangeEvent<string> name)
    {
        m_subObjective.questName = name.newValue;
    }

    void OnObjectiveChange(ChangeEvent<string> name)
    {
        m_subObjective.objectiveName = name.newValue;
    }

    void OnCompletionChange(ChangeEvent<Enum> completion)
    {
        m_subObjective.completionType = completion.newValue as QuestObjectiveCompletionType? ?? QuestObjectiveCompletionType.Completed;
    }
}
