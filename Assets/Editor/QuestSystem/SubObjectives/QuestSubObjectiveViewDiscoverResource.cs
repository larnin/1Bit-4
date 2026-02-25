using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewDiscoverResource : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveDiscoverResource m_subObjective;

    public QuestSubObjectiveViewDiscoverResource(QuestSystemNodeObjective node, QuestSubObjectiveDiscoverResource subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        element.Add(QuestSystemEditorUtility.CreateLabel("Number of block checked each second"));
        element.Add(QuestSystemEditorUtility.CreateIntField(m_subObjective.checkSpeed, "Check Speed", OnCheckSpeedChange));

        var resourceField = new EnumField("Block Type", m_subObjective.blockType);
        resourceField.RegisterValueChangedCallback(OnResourceChange);
        element.Add(resourceField);

        return element;
    }

    void OnCheckSpeedChange(ChangeEvent<int> speed)
    {
        m_subObjective.checkSpeed = speed.newValue;
    }

    void OnResourceChange(ChangeEvent<Enum> status)
    {
        m_subObjective.blockType = status.newValue as BlockType? ?? BlockType.air;
    }
}