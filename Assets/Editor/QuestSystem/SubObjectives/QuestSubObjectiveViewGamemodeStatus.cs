using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewGamemodeStatus : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveGamemodeStatus m_subObjective;

    public QuestSubObjectiveViewGamemodeStatus(QuestSystemNodeObjective node, QuestSubObjectiveGamemodeStatus subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        element.Add(QuestSystemEditorUtility.CreateTextField(m_subObjective.name, "Name", OnNameChange));

        var completion = new EnumField("Status", m_subObjective.status);
        completion.RegisterValueChangedCallback(OnStatusChange);

        return element;
    }

    void OnNameChange(ChangeEvent<string> name)
    {
        m_subObjective.name = name.newValue;
    }

    void OnStatusChange(ChangeEvent<Enum> status)
    {
        m_subObjective.status = status.newValue as GamemodeStatus? ?? GamemodeStatus.Completed;
    }
}
