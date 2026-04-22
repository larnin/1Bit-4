using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewEntityCount : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveEntityCount m_subObjective;

    public QuestSubObjectiveViewEntityCount(QuestSystemNodeObjective node, QuestSubObjectiveEntityCount subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        /*
    [SerializeField] bool m_useType = true;
    [SerializeField] string m_entityType;
    [SerializeField] bool m_useTeam = true;
    [SerializeField] Team m_team = Team.Player;
    [SerializeField] int m_count = 0;
    [SerializeField] EntityOperator m_operator = EntityOperator.Equal;
        */

        var element = new VisualElement();

        var typeElem = QuestSystemEditorUtility.CreateHorizontalLayout();
        element.Add(typeElem);
        typeElem.Add(QuestSystemEditorUtility.CreateCheckbox("", m_subObjective.useType, UseTypeChange));

        var entityNames = new List<string>();
        foreach (var e in Global.instance.editorDatas.entities)
            entityNames.Add(e.type);
        var typesField = new DropdownField("Entity type", entityNames, m_subObjective.entityType);
        typesField.RegisterValueChangedCallback(EntityTypeChange);
        typeElem.Add(typesField);

        var teamElem = QuestSystemEditorUtility.CreateHorizontalLayout();
        element.Add(teamElem);
        teamElem.Add(QuestSystemEditorUtility.CreateCheckbox("", m_subObjective.useTeam, UseTeamChange));

        var teamField = new EnumField("Team", m_subObjective.team);
        teamField.RegisterValueChangedCallback(OnTeamChange);
        element.Add(teamElem);
        teamElem.Add(teamField);

        element.Add(QuestSystemEditorUtility.CreateIntField(m_subObjective.count, "Count", OnCountChange));

        var opField = new EnumField("Operator", m_subObjective.entityOperator);
        opField.RegisterValueChangedCallback(OnOperatorChange);
        element.Add(opField);

        return element;
    }

    void UseTypeChange(ChangeEvent<bool> type)
    {
        m_subObjective.useType = type.newValue;
    }

    void EntityTypeChange(ChangeEvent<string> name)
    {
        m_subObjective.entityType = name.newValue;
    }

    void UseTeamChange(ChangeEvent<bool> team)
    {
        m_subObjective.useTeam = team.newValue;
    }

    void OnTeamChange(ChangeEvent<Enum> completion)
    {
        m_subObjective.team = completion.newValue as Team? ?? Team.Player;
    }

    void OnCountChange(ChangeEvent<int> count)
    {
        m_subObjective.count = count.newValue;
    }

    void OnOperatorChange(ChangeEvent<Enum> op)
    {
        m_subObjective.entityOperator = op.newValue as QuestSubObjectiveEntityCount.EntityOperator? ?? QuestSubObjectiveEntityCount.EntityOperator.Equal;
    }
}

