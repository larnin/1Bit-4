using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewNotImplemented : QuestSubObjectiveViewBase
{
    public QuestSubObjectiveViewNotImplemented(QuestSystemNodeObjective node, QuestSubObjectiveBase subObjective) : base(node, subObjective)
    {

    }

    public override VisualElement GetElement()
    {
        var label = QuestSystemEditorUtility.CreateLabel("<color=#FF0000>Not implemented !</color>");
        return label;
    }
}
