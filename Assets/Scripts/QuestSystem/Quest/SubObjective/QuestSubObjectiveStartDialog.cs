using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum QuestDialogEndType
{
    InputToEnd,
    InputToEndNoWait,
    KeepOpen,
    CloseAfterTimer,
    CloseAtEndObjective,
}

[Serializable]
public class QuestSubObjectiveStartDialog : QuestSubObjectiveBase
{
    [SerializeField]
    List<string> m_texts = new List<string>();

    [SerializeField]
    QuestDialogEndType m_dialogEndType = QuestDialogEndType.InputToEndNoWait;
    public QuestDialogEndType dialogEndType { get { return m_dialogEndType; } set { m_dialogEndType = value; } }

    [SerializeField]
    float m_delayToClose = 0;
    public float delayToClose { get { return m_delayToClose; } set { m_delayToClose = value; } }

    float m_timer = 0;

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
        if (m_dialogEndType == QuestDialogEndType.InputToEndNoWait || m_dialogEndType == QuestDialogEndType.KeepOpen || m_dialogEndType == QuestDialogEndType.CloseAtEndObjective)
            return true;

        if (m_dialogEndType == QuestDialogEndType.CloseAfterTimer && m_timer > m_delayToClose)
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

        DialogPopup popup = MenuSystem.instance.OpenMenu<DialogPopup>("DialogPopup", false, true, false);
        if (popup == null)
            return;

        popup.DisplayTexts(m_texts, m_dialogEndType == QuestDialogEndType.InputToEnd || m_dialogEndType == QuestDialogEndType.InputToEndNoWait);
    }

    public override void Update(float deltaTime)
    {
        if (m_dialogEndType == QuestDialogEndType.CloseAfterTimer)
        {
            if (MenuSystem.instance != null)
            {
                DialogPopup popup = MenuSystem.instance.GetOpenedMenu<DialogPopup>();
                if (popup != null && popup.IsDisplayingLastText())
                {
                    m_timer += deltaTime;
                    if(m_timer > m_delayToClose)
                        MenuSystem.instance.CloseMenu<DialogPopup>();
                }
            }
        }
    }

    public override void End()
    {
        if(m_dialogEndType == QuestDialogEndType.CloseAtEndObjective || m_dialogEndType == QuestDialogEndType.CloseAfterTimer
            )
            MenuSystem.instance.CloseMenu<DialogPopup>();
    }

}
