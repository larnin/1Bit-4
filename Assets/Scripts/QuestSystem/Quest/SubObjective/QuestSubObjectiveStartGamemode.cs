using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuestSubObjectiveStartGamemode : QuestSubObjectiveBase
{
    string m_name;
    public string name { get { return m_name; } set { m_name = value; } }

    GamemodeAssetBase m_gamemode;
    public GamemodeAssetBase gamemode { get { return m_gamemode; } set { m_gamemode = value; } }

    public override bool IsCompleted()
    {
        return true;
    }

    public override void Start()
    {
        if (GamemodeSystem.instance == null || m_gamemode == null)
            return;

        GamemodeSystem.instance.StartGamemode(m_name, m_gamemode);
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}