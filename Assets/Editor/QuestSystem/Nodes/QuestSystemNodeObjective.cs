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

    VisualElement m_subObjectiveContainer;
    VisualElement m_addSubObjectiveButton;

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

        m_subObjectiveContainer = new VisualElement();
        block.Add(m_subObjectiveContainer);
        DrawObjectivesContainer();

        m_addSubObjectiveButton = QuestSystemEditorUtility.CreateButton("Add Sub Objective", OnClickAddObjective);
        block.Add(m_addSubObjectiveButton);

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
        OnOutputChange();
    }

    public QuestObjective GetObjective()
    {
        return m_objective;
    }

    void DrawObjectivesContainer()
    {
        if (m_subObjectiveContainer == null)
            return;

        m_subObjectiveViews.Clear();
        m_subObjectiveContainer.Clear();

        for (int i = 0; i < m_objective.GetSubObjectiveCount(); i++)
        {
            var subObjective = m_objective.GetSubObjective(i);

            var view = QuestSubObjectiveViewBase.Create(this, subObjective);

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
        if(objective != null)
        {
            string name = QuestSubObjectiveBase.GetName(objective);
            var foldable = new Foldout() { text = name };
            var hierarchy = foldable.hierarchy;
            if(hierarchy.childCount > 0)
            {
                var parent = hierarchy.ElementAt(0);
                parent.style.flexDirection = FlexDirection.Row;

                var deleteButton = QuestSystemEditorUtility.CreateButton("  X", () => { DeleteSubObjective(subObjectiveView.GetSubObjective()); });
                deleteButton.style.width = 15;
                parent.Add(deleteButton);
            }
            box.Add(foldable);

            var element = subObjectiveView.GetElement();
            if(element != null)
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
        if (m_objective == null)
            return;

        m_objective.AddSubObjective(objective);
        OnOutputChange();

        DrawObjectivesContainer();
    }

    void DeleteSubObjective(QuestSubObjectiveBase objective)
    {
        m_objective.RemoveSubObjective(objective);
        OnOutputChange();

        DrawObjectivesContainer();
    }

    void MoveSubObjective(QuestSubObjectiveBase objective, int offset)
    {
        int index = GetSubObjectiveIndex(objective);

        if (index < 0)
            return;

        int newIndex = index + offset;
        if (newIndex < 0 || index >= m_objective.GetSubObjectiveCount())
            return;

        m_objective.RemoveSubObjective(objective);
        if (offset > 0)
            newIndex--;
        m_objective.InsertSubObjectiveAt(objective, index);
        OnOutputChange();

        DrawObjectivesContainer();
    }

    int GetSubObjectiveIndex(QuestSubObjectiveBase objective)
    {
        int nbObjective = m_objective.GetSubObjectiveCount();

        for (int i = 0; i < nbObjective; i++)
        {
            if (objective == m_objective.GetSubObjective(i))
                return i;
        }

        return -1;
    }

    public void OnOutputChange()
    {
        List<string> outPorts = new List<string>();

        int nbSubObjective = m_objective.GetSubObjectiveCount();
        for(int i = 0; i < nbSubObjective; i++)
        {
            var subObjective = m_objective.GetSubObjective(i);
            AddFailPort(subObjective, outPorts);
        }

        List<string> currentPorts = new List<string>();
        int nbPort = outputContainer.childCount;
        for(int i = 0; i < nbPort; i++)
        {
            var port = outputContainer[i] as Port;
            if (port == null || port.portName == "Out")
                continue;

            if (outPorts.Contains(port.portName))
            {
                currentPorts.Add(port.portName);
                continue;
            }

            List<Edge> edgeToRemove = new List<Edge>();
            foreach(var c in port.connections)
                edgeToRemove.Add(c);

            m_graphView.DeleteElements(edgeToRemove);

            outputContainer.RemoveAt(i);
            i--;
            nbPort--;
        }

        foreach(var port in outPorts)
        {
            if (currentPorts.Contains(port))
                continue;

            Port outputPort = this.CreatePort(port, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            outputContainer.Add(outputPort);
        }
    }

    void AddFailPort(QuestSubObjectiveBase subObjective, List<string> outPorts)
    {
        if (subObjective.CanFail() & subObjective.failNodeName.Length != 0)
        {
            if (!outPorts.Contains(subObjective.failNodeName))
                outPorts.Add(subObjective.failNodeName);
        }

        int subObjectiveNb = subObjective.GetSubObjectiveCount();
        for(int i = 0; i < subObjectiveNb; i++)
            AddFailPort(subObjective.GetSubObjective(i), outPorts);
    }
}
