using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class QuestSubObjectiveBase
{
    protected QuestSystem m_system;

    public abstract void Start();
    public abstract void Update(float deltaTime);
    public abstract void End();

    public abstract bool IsCompleted();

    public void SetSystem(QuestSystem system)
    {
        m_system = system;
    }

    public QuestSystem GetSystem()
    {
        return m_system;
    }
}
