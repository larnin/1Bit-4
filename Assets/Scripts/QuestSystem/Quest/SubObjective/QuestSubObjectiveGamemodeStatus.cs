using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestSubObjectiveGamemodeStatus : QuestSubObjectiveBase
{
    [SerializeField]
    string m_name;
    public string name { get { return m_name; } set { m_name = value; } }

    [SerializeField]
    GamemodeStatus m_status;
    public GamemodeStatus status { get { return m_status; } set { m_status = value; } }

    public override bool IsCompleted()
    {
        if (GamemodeSystem.instance == null)
            return false;

        GamemodeBase gamemode = GamemodeSystem.instance.GetGamemode(m_name);
        if (gamemode == null)
            return false;

        return gamemode.GetStatus() == m_status;
    }

    public override void Start() { }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
