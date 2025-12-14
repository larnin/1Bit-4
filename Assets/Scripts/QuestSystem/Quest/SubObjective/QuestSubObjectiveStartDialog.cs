using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestSubObjectiveStartDialog : QuestSubObjectiveBase
{
    [SerializeField]
    List<string> m_texts = new List<string>();

    [SerializeField]
    bool m_inputToEndDialog = true;
    public bool inputToEndDialog { get { return m_inputToEndDialog; } set { m_inputToEndDialog = value; } }

    [SerializeField]
    bool m_waitDialogEndToComplete = false;
    public bool waitDialogEndToComplete { get { return m_waitDialogEndToComplete; } set { m_waitDialogEndToComplete = value; } }

    public int GetTextCount() { return m_texts.Count; }

    public string GetTextAt(int index)
    {
        if (index < 0 || index >= m_texts.Count)
            return "";
        return m_texts[index];
    }

    public void SetTextAt(int index, string text)
    {
        if (index < 0 || index >= m_texts.Count)
            return;
        m_texts[index] = text;
    }

    public void AddNewText()
    {
        m_texts.Add("");
    }

    public void RemoveTextAt(int index)
    {
        if (index < 0 || index >= m_texts.Count)
            return;
        m_texts.RemoveAt(index);
    }

    public override bool IsCompleted()
    {
        if (!m_waitDialogEndToComplete)
            return true;

        if (MenuSystem.instance == null)
            return true;

        if (MenuSystem.instance.GetOpenedMenu<DialogPopup>() != null)
            return false;

        return true;
    }

    public override void Start()
    {
        if (MenuSystem.instance == null)
            return;

        if (m_texts.Count == 0)
            return;

        DialogPopup popup = MenuSystem.instance.OpenMenu<DialogPopup>("DialogPopup");
        if (popup == null)
            return;

        popup.DisplayTexts(m_texts, m_inputToEndDialog);
    }

    public override void Update(float deltaTime) { }

    public override void End() { }

}
