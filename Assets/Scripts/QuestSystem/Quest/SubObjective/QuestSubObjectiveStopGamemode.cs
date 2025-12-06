using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuestSubObjectiveStopGamemode : QuestSubObjectiveBase
{
    string m_name;
    public string name { get { return m_name; } set { m_name = value; } }

    public override bool IsCompleted()
    {
        return true;   
    }

    public override void Start()
    {
        if (GamemodeSystem.instance == null)
            return;

        GamemodeSystem.instance.StopGamemode(m_name);
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
