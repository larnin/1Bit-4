using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewSetBuildingUnlocked : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveSetBuildingUnlocked m_subObjective;

    public QuestSubObjectiveViewSetBuildingUnlocked(QuestSystemNodeObjective node, QuestSubObjectiveSetBuildingUnlocked subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        var completion = new EnumField("Building Type", m_subObjective.buildingType);
        completion.RegisterValueChangedCallback(OnBuildingChange);
        element.Add(completion);

        element.Add(QuestSystemEditorUtility.CreateCheckbox("Unlocked", m_subObjective.unlocked, OnLockChange));

        return element;
    }

    void OnBuildingChange(ChangeEvent<Enum> completion)
    {
        m_subObjective.buildingType = completion.newValue as BuildingType? ?? BuildingType.Pylon;
    }

    void OnLockChange(ChangeEvent<bool> unlocked)
    {
        m_subObjective.unlocked = unlocked.newValue;
    }
}
