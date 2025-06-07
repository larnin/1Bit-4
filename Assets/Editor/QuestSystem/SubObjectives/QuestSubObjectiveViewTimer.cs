using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewTimer : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveTimer m_subObjective;

    public QuestSubObjectiveViewTimer(QuestSystemNodeObjective node, QuestSubObjectiveTimer subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    public override VisualElement GetElement()
    {
        return QuestSystemEditorUtility.CreateFloatField(m_subObjective.duration, "Duration", OnDurationChange);
    }

    void OnDurationChange(ChangeEvent<float> newDuration)
    {
        m_subObjective.duration = newDuration.newValue;
    }
}

