using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewIsLevelCompleted : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveIsLevelCompleted m_subObjective;

    public QuestSubObjectiveViewIsLevelCompleted(QuestSystemNodeObjective node, QuestSubObjectiveIsLevelCompleted subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();
        element.Add(QuestSystemEditorUtility.CreateTextField(m_subObjective.levelName, "Level Name", OnQuestChange));

        var completion = new EnumField("Completion Type", m_subObjective.completionType);
        completion.RegisterValueChangedCallback(OnCompletionChange);
        element.Add(completion);

        return element;
    }

    void OnQuestChange(ChangeEvent<string> name)
    {
        m_subObjective.levelName = name.newValue;
    }

    void OnCompletionChange(ChangeEvent<Enum> completion)
    {
        m_subObjective.completionType = completion.newValue as QuestObjectiveCompletionType? ?? QuestObjectiveCompletionType.Completed;
    }
}
