using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public abstract class QuestSubObjectiveBase
{
    [SerializeField] List<int> m_detailSockets = new List<int>();

    protected QuestSystem m_system;

    protected string m_failNodeName = "";
    public string failNodeName { get { if (m_failNodeName == null) return " "; return m_failNodeName; } set { m_failNodeName = value; } }

    public abstract void Start();
    public abstract void Update(float deltaTime);
    public abstract void End();

    public abstract bool IsCompleted();

    public virtual bool CanFail() { return false; }
    public virtual bool IsFail() { return false; }

    public virtual int GetSubObjectiveCount() { return 0; }
    public virtual QuestSubObjectiveBase GetSubObjective(int index) { return null; }

    public virtual int GetDetailCount() { return 0; }
    public virtual string GetDetailName(int index) { return ""; }
    public virtual string GetDetail(int index) { return ""; }

    public int GetDetailSocket(int index)
    {
        if (m_detailSockets.Count() < index || index < 0)
            return -1;
        return m_detailSockets[index];
    }

    public void SetDetailSocket(int index, int value)
    {
        if (index < 0 || index >= GetDetailCount())
            return;

        while (m_detailSockets.Count() <= index)
            m_detailSockets.Add(-1);
        m_detailSockets[index] = value;
    }

    public void SetSystem(QuestSystem system)
    {
        m_system = system;
    }

    public QuestSystem GetSystem()
    {
        return m_system;
    }


    public static string GetName(QuestSubObjectiveBase subObjective)
    {
        return GetName(subObjective.GetType());
    }

    public static string GetName(Type type)
    {
        const string startString = "QuestSubObjective";

        string name = type.Name;
        if (name.StartsWith(startString))
            name = name.Substring(startString.Length);

        return name;
    }
}
