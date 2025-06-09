using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuestSubObjectiveGroup : QuestSubObjectiveBase
{
    public enum Operator
    {
        OR,
        AND,
        NAND,
        NOR,
    }

    Operator m_completionOperator = Operator.AND;
    public Operator completionOperator { get { return m_completionOperator; } set { m_completionOperator = value; } }

    List<QuestSubObjectiveBase> m_subObjectives = new List<QuestSubObjectiveBase>();

    public override int GetSubObjectiveCount()
    {
        return m_subObjectives.Count;
    }

    public override QuestSubObjectiveBase GetSubObjective(int index)
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


    public override bool IsCompleted()
    {
        int nbCompleted = 0;

        foreach(var sub in m_subObjectives)
        {
            if (sub.IsCompleted())
                nbCompleted++;
        }

        switch(m_completionOperator)
        {
            case Operator.OR:
                return nbCompleted > 0;
            case Operator.AND:
                return nbCompleted == m_subObjectives.Count;
            case Operator.NAND:
                return nbCompleted != m_subObjectives.Count;
            case Operator.NOR:
                return nbCompleted == 0;
            default:
                break;
        }

        return false;
    }

    public override void Start()
    {
        foreach (var sub in m_subObjectives)
        {
            sub.Start();
        }
    }

    public override void Update(float deltaTime)
    {
        foreach (var sub in m_subObjectives)
        {
            sub.Update(deltaTime);
        }
    }
    
    public override void End()
    {
        foreach (var sub in m_subObjectives)
        {
            sub.End();
        }
    }

}
