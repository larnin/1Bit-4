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
}
