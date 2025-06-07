using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum QuestOperator
{
    OR,
    AND,
}

public class QuestObjective
{
    [SerializeField] QuestOperator m_multipleInputOperator = QuestOperator.AND;
    public QuestOperator multipleInputOperator { get { return m_multipleInputOperator; } set { m_multipleInputOperator = value; } }

    [SerializeField] List<QuestSubObjectiveBase> m_subObjectives = new List<QuestSubObjectiveBase>();

    protected QuestSystem m_system;

    public bool IsCompleted()
    {
        foreach(var sub in m_subObjectives)
        {
            if (!sub.IsCompleted())
                return false;
        }

        return true;
    }

    public void Start()
    {
        foreach (var sub in m_subObjectives)
        {
            sub.Start();
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var sub in m_subObjectives)
        {
            sub.Update(deltaTime);
        }
    }

    public void End()
    {
        foreach (var sub in m_subObjectives)
        {
            sub.End();
        }
    }

    public int GetSubObjectiveCount()
    {
        return m_subObjectives.Count;
    }

    public QuestSubObjectiveBase GetSubObjective(int index)
    {
        if (index < 0 || index >= m_subObjectives.Count)
            return null;

        return m_subObjectives[index];
    }

    public void AddSubObjective(QuestSubObjectiveBase objective)
    {
        m_subObjectives.Add(objective);
    }

    public void RemoveSubObjective(QuestSubObjectiveBase objective)
    {
        m_subObjectives.Remove(objective);
    }

    public void RemoveSubObjectiveAt(int index)
    {
        m_subObjectives.RemoveAt(index);
    }

    public void InsertSubObjectiveAt(QuestSubObjectiveBase objective, int index)
    {
        if (index < 0)
            return;

        if (index >= m_subObjectives.Count)
            return;

        m_subObjectives.Insert(index, objective);
    }
}
