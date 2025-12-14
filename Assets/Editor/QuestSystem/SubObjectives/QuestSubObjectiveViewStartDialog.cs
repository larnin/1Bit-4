using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class QuestSubObjectiveViewStartDialog : QuestSubObjectiveViewBase
{
    new QuestSubObjectiveStartDialog m_subObjective;

    VisualElement m_textsContainer;

    public QuestSubObjectiveViewStartDialog(QuestSystemNodeObjective node, QuestSubObjectiveStartDialog subObjective) : base(node, subObjective)
    {
        m_subObjective = subObjective;
    }

    protected override VisualElement GetElementInternal()
    {
        var element = new VisualElement();

        VisualElement DialogEnd = QuestSystemEditorUtility.CreateCheckbox("Wait dialog end", m_subObjective.waitDialogEndToComplete, WaitDialogEndComplete);
        DialogEnd.tooltip = "If not checked, this sub objective is completed instantly";
        element.Add(DialogEnd);

        VisualElement NeedPlayerInput = QuestSystemEditorUtility.CreateCheckbox("Need player input", m_subObjective.inputToEndDialog, InputEndDialog);
        NeedPlayerInput.tooltip = "If not checked, this dialog will need to be closed manually with an other quest objective";
        element.Add(NeedPlayerInput);

        m_textsContainer = new VisualElement();
        element.Add(m_textsContainer);
        UpdateTextsList();

        element.Add(QuestSystemEditorUtility.CreateButton("Add", AddTextClick));

        return element;
    }

    void WaitDialogEndComplete(ChangeEvent<bool> value)
    {
        m_subObjective.waitDialogEndToComplete = value.newValue;
    }

    void InputEndDialog(ChangeEvent<bool> value)
    {
        m_subObjective.inputToEndDialog = value.newValue;
    }

    void AddTextClick()
    {
        m_subObjective.AddNewText();
        UpdateTextsList();
    }

    void RemoveText(int index)
    {
        m_subObjective.RemoveTextAt(index);
        UpdateTextsList();
    }

    void TextUpdate(ChangeEvent<string> text, int index)
    {
        m_subObjective.SetTextAt(index, text.newValue);
    }

    void UpdateTextsList()
    {
        if (m_textsContainer == null)
            return;

        m_textsContainer.Clear();

        int nbText = m_subObjective.GetTextCount();
        for(int i = 0; i < nbText; i++)
        {
            VisualElement elem = new VisualElement();
            elem.style.flexDirection = FlexDirection.Row;

            int index = i;

            elem.Add(QuestSystemEditorUtility.CreateTextArea(m_subObjective.GetTextAt(i), null, (ChangeEvent<string> newValue) => { TextUpdate(newValue, index); }));

            var deleteButton = QuestSystemEditorUtility.CreateButton("  X", () => { RemoveText(index); });
            deleteButton.style.width = 15;
            elem.Add(deleteButton);

            m_textsContainer.Add(elem);
        }
    }
}
