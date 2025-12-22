using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public  class QuestSubObjectiveViewDestroyObject : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveDestroyObject m_subObjective;

    public QuestSubObjectiveViewDestroyObject(QuestSystemNodeObjective node, QuestSubObjectiveDestroyObject subObjective) : base(node, subObjective)
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
