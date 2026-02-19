using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewHaveResource : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveHaveResource m_subObjective;

    public QuestSubObjectiveViewHaveResource(QuestSystemNodeObjective node, QuestSubObjectiveHaveResource subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }


    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        var resourceField = new EnumField("Resource", m_subObjective.resourceType);
        resourceField.RegisterValueChangedCallback(OnResourceChange);
        element.Add(resourceField);

        var operatorField = new EnumField("Operator", m_subObjective.valueOperator);
        operatorField.RegisterValueChangedCallback(OnOperatorChange);
        element.Add(operatorField);

        element.Add(QuestSystemEditorUtility.CreateFloatField(m_subObjective.quantity, "Quantity", OnQuantityChange));

        return element;
    }

    void OnResourceChange(ChangeEvent<Enum> status)
    {
        m_subObjective.resourceType = status.newValue as ResourceType? ?? ResourceType.Energy;
    }

    void OnOperatorChange(ChangeEvent<Enum> status)
    {
        m_subObjective.valueOperator = status.newValue as QuestSubObjectiveHaveResource.ValueOperator? ?? QuestSubObjectiveHaveResource.ValueOperator.MoreOrEqual;
    }

    void OnQuantityChange(ChangeEvent<float> quantity)
    {
        m_subObjective.quantity = quantity.newValue;
    }
}
