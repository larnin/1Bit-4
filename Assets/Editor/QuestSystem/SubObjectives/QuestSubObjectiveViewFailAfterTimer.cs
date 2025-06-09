using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewFailAfterTimer : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveFailAfterTimer m_subObjective;

    public QuestSubObjectiveViewFailAfterTimer(QuestSystemNodeObjective node, QuestSubObjectiveFailAfterTimer subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        return QuestSystemEditorUtility.CreateFloatField(m_subObjective.duration, "Duration", OnDurationChange);
    }

    void OnDurationChange(ChangeEvent<float> newDuration)
    {
        m_subObjective.duration = newDuration.newValue;
    }
}
