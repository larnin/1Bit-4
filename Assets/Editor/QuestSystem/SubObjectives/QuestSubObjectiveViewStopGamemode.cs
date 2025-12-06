using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewStopGamemode : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveStopGamemode m_subObjective;

    public QuestSubObjectiveViewStopGamemode(QuestSystemNodeObjective node, QuestSubObjectiveStopGamemode subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        return QuestSystemEditorUtility.CreateTextField(m_subObjective.name, "Name", OnNameChange);
    }

    void OnNameChange(ChangeEvent<string> name)
    {
        m_subObjective.name = name.newValue;
    }
}
