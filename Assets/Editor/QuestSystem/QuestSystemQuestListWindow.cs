using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestSystemQuestListWindow
{
    QuestSystemGraph m_editor;
    VisualElement m_parent;

    List<string> m_lastActiveQuest = new List<string>();

    public void SetParent(QuestSystemGraph editor, VisualElement parent)
    {
        m_parent = parent;
        m_editor = editor;
    }

    public void Update(bool playing)
    {
        if(!playing)
        {
            m_lastActiveQuest.Clear();
            m_parent.Clear();
            m_parent.style.height = 1;
            return;
        }

        if (QuestSystem.instance == null)
            return;

        var questNames = QuestSystem.instance.GetActiveQuestsNames();
        questNames.AddRange(QuestSystem.instance.GetCompletedQuestNames());

        if (!HaveActiveListChanged(questNames))
            return;
        m_lastActiveQuest = questNames;

        m_parent.style.height = 100;

        m_parent.Clear();
        m_parent.Add(QuestSystemEditorUtility.CreateLabel("Active quests :"));

        foreach (var name in questNames)
            m_parent.Add(QuestSystemEditorUtility.CreateButton(name, ()=> { OnClicQuest(name); }));
    }

    void OnClicQuest(string name)
    {
        if (QuestSystem.instance == null)
            return;

        var obj = QuestSystem.instance.GetQuestObject(name);
        if (obj == null)
            return;

        if (m_editor == null)
            return;

        m_editor.Load(obj);
    }

    bool HaveActiveListChanged(List<string> newList)
    {
        foreach(var e in newList)
        {
            if (!m_lastActiveQuest.Contains(e))
                return true;
        }

        foreach(var e in m_lastActiveQuest)
        {
            if (!newList.Contains(e))
                return true;
        }

        return false;
    }
}
