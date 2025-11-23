using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif


[Serializable]
[InlineProperty(LabelWidth = 13)]
public abstract class ChoiceString
{
    [HideInInspector]
    [SerializeField] string m_value = "";

#if UNITY_EDITOR
    Rect m_setupRect;
#endif

    public string GetValue()
    {
        return m_value;
    }

    public void SetValue(string value)
    {
#if UNITY_EDITOR
        m_value = value;
        EditorUtility.SetDirty(Selection.activeObject);
#endif
    }

    [OnInspectorGUI]
    private void OnInspectorGUI()
    {
#if UNITY_EDITOR
        var choices = GetChoices();

        if(m_value != "")
        {
            if (m_value == null || !choices.Contains(m_value))
                m_value = "";
        }

        if (m_value == "")
        {
            EditorGUILayout.HelpBox("Value not Set", MessageType.Warning);
            GUILayout.BeginHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(m_value);
        }

        if (GUILayout.Button("Setup", GUILayout.Width(100)))
        {
            PopupWindow.Show(m_setupRect, new ChoiceStringPopup(this, choices));
        }
        if (Event.current.type == EventType.Repaint)
            m_setupRect = GUILayoutUtility.GetLastRect();

        GUILayout.EndHorizontal();
#endif
    }

    protected abstract List<string> GetChoices();
}


#if UNITY_EDITOR
class ChoiceStringPopup : PopupWindowContent
{
    string m_filter = "";
    ChoiceString m_value = null;
    List<string> m_choices = null;

    Vector2 m_scrollPos = Vector2.zero;

    public ChoiceStringPopup(ChoiceString value, List<string> choices)
    {
        m_value = value;
        m_choices = choices;
    }

    public override void OnGUI(Rect rect)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Filter:", GUILayout.Width(40));
        m_filter = GUILayout.TextField(m_filter);
        GUILayout.EndHorizontal();

        m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
        int nbChoices = m_choices.Count();
        for (int i = 0; i < nbChoices; i++)
        {
            if (!TextUtility.ProcessFilter(m_choices[i], m_filter))
                continue;

            if (GUILayout.Button(m_choices[i]))
            {
                m_value.SetValue(m_choices[i]);
                editorWindow.Close();
            }
        }
        GUILayout.EndScrollView();
    }
}
#endif