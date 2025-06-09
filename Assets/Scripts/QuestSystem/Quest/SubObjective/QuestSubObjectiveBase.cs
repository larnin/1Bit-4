using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class QuestSubObjectiveBase
{
    protected QuestSystem m_system;

    protected string m_failNodeName = "";
    public string failNodeName { get { return m_failNodeName; } set { m_failNodeName = value; } }

    public abstract void Start();
    public abstract void Update(float deltaTime);
    public abstract void End();

    public abstract bool IsCompleted();

    public virtual bool CanFail() { return false; }
    public virtual bool IsFail() { return false; }

    public virtual int GetSubObjectiveCount() { return 0; }
    public virtual QuestSubObjectiveBase GetSubObjective(int index) { return null; }

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
