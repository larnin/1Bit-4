using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QuestSystem : SerializedMonoBehaviour
{
    static QuestSystem m_instance = null;
    public static QuestSystem instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }

    public void StartQuest(QuestSaveData data)
    {
        //todo
    }

    public void StopQuest(string name)
    {
        //todo
    }

    public bool IsQuestActive(string name)
    {
        //todo
        return false;
    }

    public bool IsQuestObjectiveActive(string quest, string objective)
    {
        //todo
        return false;
    }
}
