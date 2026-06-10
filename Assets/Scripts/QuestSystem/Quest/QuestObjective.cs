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

public class QuestObjectiveText
{
    public string title;
    public List<string> details = new List<string>();
}

public class QuestObjective
{
    [SerializeField] QuestObjectiveText m_text = new QuestObjectiveText();
    public QuestObjectiveText text { get { return m_text; } }

    [SerializeField] QuestOperator m_multipleInputOperator = QuestOperator.AND;
    public QuestOperator multipleInputOperator { get { return m_multipleInputOperator; } set { m_multipleInputOperator = value; } }

    [SerializeField] List<QuestSubObjectiveBase> m_subObjectives = new List<QuestSubObjectiveBase>();

    public bool IsCompleted()
    {
        foreach(var sub in m_subObjectives)
        {
            if (!sub.IsCompleted())
                return false;
        }

        return true;
    }

    public bool IsFail()
    {
        foreach(var sub in m_subObjectives)
        {
            if (!sub.CanFail() || sub.failNodeName.Length == 0)
                continue;

            if (sub.IsFail())
                return true;
        }

        return false;
    }

    public string GetFailNode()
    {
        foreach (var sub in m_subObjectives)
        {
            if (!sub.CanFail() || sub.failNodeName.Length == 0)
                continue;

            return sub.failNodeName;
        }

        return "";
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

    public QuestObjectiveText GetCompletedTexts()
    {
        QuestObjectiveText newTexts = new QuestObjectiveText();
        newTexts.title = CompleteText(m_text.title);
        foreach (var d in m_text.details)
            newTexts.details.Add(CompleteText(d));

        return newTexts;
    }

    string CompleteText(string text)
    {
        string newText = String.Copy(text);
        int lastIndex = 0;
        while (true)
        {
            int nextIndex = newText.IndexOf('[', lastIndex);
            if (nextIndex < 0)
                return newText;

            int closeIndex = newText.IndexOf(']', nextIndex);
            if (closeIndex < 0)
                return newText;

            string valueStr = newText.Substring(nextIndex + 1, closeIndex - nextIndex - 2);
            int socket;
            if(!int.TryParse(valueStr, out socket))
            {
                lastIndex = closeIndex + 1;
                continue;
            }

            string detail = "";
            if(!GetDetailForSocket(socket, ref detail))
            {
                lastIndex = closeIndex + 1;
                continue;
            }

            newText.Remove(nextIndex, closeIndex - nextIndex + 1);
            newText.Insert(nextIndex, detail);
            lastIndex = nextIndex + detail.Length;
        }
    }

    bool GetDetailForSocket(int socket, ref string value)
    {
        foreach(var obj in m_subObjectives)
        {
            if (GetDetailForSocket(obj, socket, ref value))
                return true;
        }

        return false;
    }

    bool GetDetailForSocket(QuestSubObjectiveBase objective, int socket, ref string value)
    {
        int nbSub = objective.GetSubObjectiveCount();
        for(int i = 0; i < nbSub; i++)
        {
            if (GetDetailForSocket(objective.GetSubObjective(i), socket, ref value))
                return true;
        }

        int nbDetail = objective.GetDetailCount();
        for(int i = 0; i < nbDetail; i++)
        {
            if(objective.GetDetailSocket(i) == socket)
            {
                value = objective.GetDetail(i);
                return true;
            }
        }

        return false;
    }
}
