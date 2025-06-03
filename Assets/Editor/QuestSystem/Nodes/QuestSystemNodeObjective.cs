using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSystemNodeObjective : QuestSystemNode
{
    QuestObjective m_objective = new QuestObjective();
    List<QuestSubObjectiveViewBase> m_subObjectiveViews = new List<QuestSubObjectiveViewBase>();

    public override void Draw()
    {
        base.Draw();

        Port inputPort = this.CreatePort("In", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
        inputContainer.Add(inputPort);

        Port outputPort = this.CreatePort("Out", Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
        outputContainer.Add(outputPort);
    }

    public override VisualElement GetDetailElement()
    {
        VisualElement block = new VisualElement();

        block.Add(GetInputOperator());

        return block;
    }

    VisualElement GetInputOperator()
    {
        var names = Enum.GetNames(typeof(QuestOperator)).ToList();

        var dropDown = new EnumField("Multi Input Operator", m_objective.multipleInputOperator);
        dropDown.RegisterValueChangedCallback(InputOperatorChanged);

        return dropDown;
    }
    
    void InputOperatorChanged(ChangeEvent<Enum> value)
    {
        m_objective.multipleInputOperator = value.newValue as QuestOperator? ?? QuestOperator.AND;
    }

    public void SetObjective(QuestObjective objective)
    {
        m_objective = objective;
    }
}
