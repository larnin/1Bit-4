using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class QuestSystemAddSubObjectivePopup : PopupWindowContent
{
    Action<QuestSubObjectiveBase> m_callback;

    string m_filter = "";
    Vector2 m_scrollPos = Vector2.zero;

    public QuestSystemAddSubObjectivePopup(Action<QuestSubObjectiveBase> callback)
    {
        m_callback = callback;
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.Label("Add Sub Objective");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Filter:", GUILayout.Width(40));
        m_filter = GUILayout.TextField(m_filter);
        GUILayout.EndHorizontal();

        var types = typeof(QuestSubObjectiveBase).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(QuestSubObjectiveBase)));

        m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
        foreach (var t in types)
        {
            string name = QuestSubObjectiveBase.GetName(t);

            if (!TextUtility.ProcessFilter(name, m_filter))
                continue;

            if (GUILayout.Button(name))
            {
                SendSubObjective(t);
                editorWindow.Close();
            }
        }
        GUILayout.EndScrollView();
    }

    void SendSubObjective(Type type)
    {
        if (m_callback == null)
            return;

        var objective = Activator.CreateInstance(type) as QuestSubObjectiveBase;
        if (objective != null)
            m_callback(objective);
    }
}

