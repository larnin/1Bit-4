using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewGroup : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveGroup m_subObjective;
    List<QuestSubObjectiveViewBase> m_subObjectiveViews = new List<QuestSubObjectiveViewBase>();

    VisualElement m_subObjectiveContainer;
    VisualElement m_addSubObjectiveButton;

    public QuestSubObjectiveViewGroup(QuestSystemNodeObjective node, QuestSubObjectiveGroup subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    public override VisualElement GetElement()
    {
        VisualElement block = new VisualElement();

        block.Add(GetCompletionOperator());

        m_subObjectiveContainer = new VisualElement();
        block.Add(m_subObjectiveContainer);
        DrawObjectivesContainer();

        m_addSubObjectiveButton = QuestSystemEditorUtility.CreateButton("Add Sub Objective", OnClickAddObjective);
        block.Add(m_addSubObjectiveButton);

        return block;
    }

    VisualElement GetCompletionOperator()
    {
        var names = Enum.GetNames(typeof(QuestOperator)).ToList();

        var dropDown = new EnumField("Multi Input Operator", m_subObjective.completionOperator);
        dropDown.RegisterValueChangedCallback(CompletionOperatorChanged);

        return dropDown;
    }

    void CompletionOperatorChanged(ChangeEvent<Enum> value)
    {
        m_subObjective.completionOperator = value.newValue as QuestSubObjectiveGroup.Operator? ?? QuestSubObjectiveGroup.Operator.AND;
    }

    void DrawObjectivesContainer()
    {
        if (m_subObjectiveContainer == null)
            return;

        m_subObjectiveViews.Clear();
        m_subObjectiveContainer.Clear();

        for (int i = 0; i < m_subObjective.GetSubObjectiveCount(); i++)
        {
            var subObjective = m_subObjective.GetSubObjective(i);

            var view = QuestSubObjectiveViewBase.Create(m_node, subObjective);

            if (view != null)
            {
                m_subObjectiveViews.Add(view);
                var subObjectiveContainer = DrawOneSubObjective(view);
                if (subObjectiveContainer == null)
                    continue;

                m_subObjectiveContainer.Add(subObjectiveContainer);
            }
        }
    }

    VisualElement DrawOneSubObjective(QuestSubObjectiveViewBase subObjectiveView)
    {
        var box = new Box();
        QuestSystemEditorUtility.SetContainerStyle(box, 2, new Color(0.6f, 0.6f, 0.6f), 1, 3, new Color(0.2f, 0.2f, 0.2f, 0.1f));

        var objective = subObjectiveView.GetSubObjective();
        if (objective != null)
        {
            string name = QuestSubObjectiveBase.GetName(objective);
            var foldable = new Foldout() { text = name };
            var hierarchy = foldable.hierarchy;
            if (hierarchy.childCount > 0)
            {
                var parent = hierarchy.ElementAt(0);
                parent.style.flexDirection = FlexDirection.Row;

                var deleteButton = QuestSystemEditorUtility.CreateButton("  X", () => { DeleteSubObjective(subObjectiveView.GetSubObjective()); });
                deleteButton.style.width = 15;
                parent.Add(deleteButton);
            }
            box.Add(foldable);

            var element = subObjectiveView.GetElement();
            if (element != null)
                foldable.Add(element);
        }


        return box;
    }

    void OnClickAddObjective()
    {
        if (m_addSubObjectiveButton == null)
            return;

        var pos = m_addSubObjectiveButton.LocalToWorld(new Vector2(0, 0));
        pos.y -= 100;
        var rect = new Rect(pos, new Vector2(200, 100));

        UnityEditor.PopupWindow.Show(rect, new QuestSystemAddSubObjectivePopup(ReceiveNewSubObjective));
    }

    void ReceiveNewSubObjective(QuestSubObjectiveBase objective)
    {
        if (m_subObjective == null)
            return;

        m_subObjective.AddSubObjective(objective);

        DrawObjectivesContainer();
    }

    void DeleteSubObjective(QuestSubObjectiveBase objective)
    {
        m_subObjective.RemoveSubObjective(objective);
        DrawObjectivesContainer();
    }
}
