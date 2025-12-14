using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewEmpty : QuestSubObjectiveViewBase
{
    public QuestSubObjectiveViewEmpty(QuestSystemNodeObjective node, QuestSubObjectiveBase subObjective) : base(node, subObjective)
    {

    }

    protected override VisualElement GetElementInternal()
    {
        var label = QuestSystemEditorUtility.CreateLabel("No parameters");
        return label;
    }
}