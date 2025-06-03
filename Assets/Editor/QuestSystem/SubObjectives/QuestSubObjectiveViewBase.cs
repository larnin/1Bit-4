using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public abstract class QuestSubObjectiveViewBase
{
    public static QuestSubObjectiveViewBase Create(QuestSystemNodeObjective node, QuestSubObjectiveBase objective)
    {
        return null;
    }

    public abstract VisualElement GetElement();
}
