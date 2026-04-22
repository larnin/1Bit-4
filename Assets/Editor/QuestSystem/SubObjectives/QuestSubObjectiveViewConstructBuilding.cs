using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewConstructBuilding : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveConstructBuilding m_subObjective;

    public QuestSubObjectiveViewConstructBuilding(QuestSystemNodeObjective node, QuestSubObjectiveConstructBuilding subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        var completion = new EnumField("Building Type", m_subObjective.buildingType);
        completion.RegisterValueChangedCallback(OnBuildingChange);
        element.Add(completion);

        element.Add(QuestSystemEditorUtility.CreateIntField(m_subObjective.count, "Count", OnCountChange));

        return element;
    }

    void OnBuildingChange(ChangeEvent<Enum> completion)
    {
        m_subObjective.buildingType = completion.newValue as BuildingType? ?? BuildingType.Pylon;
    }

    void OnCountChange(ChangeEvent<int> count)
    {
        m_subObjective.count = count.newValue;
    }
}
